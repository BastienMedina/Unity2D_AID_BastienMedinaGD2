using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

// Gère l'état des patterns laser et la détection de collision.
public class LaserSystem : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Références externes
    // -------------------------------------------------------------------------

    // Référence au gestionnaire de tour pour s'abonner.
    [SerializeField] private TurnManager _turnManager;

    // Référence au gestionnaire de grille pour la position joueur.
    [SerializeField] private GridManager _gridManager;

    // -------------------------------------------------------------------------
    // Données configurables
    // -------------------------------------------------------------------------

    // Séquence ordonnée de patterns laser data-driven.
    [SerializeField] private List<LaserPattern> _patterns = new List<LaserPattern>();

    // -------------------------------------------------------------------------
    // Événements publics
    // -------------------------------------------------------------------------

    // Déclenché quand le joueur est touché par le laser.
    public UnityEvent OnPlayerHitByLaser = new UnityEvent();

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    // Index du pattern laser actuellement actif.
    private int _currentPatternIndex;

    // Liste mutable des cellules laser actives ce tour.
    private List<Vector2Int> _activeLaserCells = new List<Vector2Int>();

    // Indique si la séquence de patterns est utilisable.
    private bool HasValidPatterns => _patterns != null && _patterns.Count > 0;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Valide les références et s'abonne aux événements de tour.
    private void Awake()
    {
        // Lève une exception si TurnManager n'est pas assigné.
        if (_turnManager == null)
        {
            throw new MissingReferenceException(
                $"[LaserSystem] La référence {nameof(_turnManager)} n'est pas assignée."
            );
        }

        // Lève une exception si GridManager n'est pas assigné.
        if (_gridManager == null)
        {
            throw new MissingReferenceException(
                $"[LaserSystem] La référence {nameof(_gridManager)} n'est pas assignée."
            );
        }

        // Abonne l'évaluation à l'événement dédié du TurnManager.
        _turnManager.OnEvaluateLaser.AddListener(HandleEvaluateLaser);

        // Abonne l'avancement à l'événement dédié du TurnManager.
        _turnManager.OnAdvanceLaser.AddListener(HandleAdvanceLaser);

        // Initialise l'index au premier pattern de la séquence.
        _currentPatternIndex = 0;

        // Charge les cellules actives du pattern initial en mémoire.
        RebuildActiveCells();
    }

    // Désabonne proprement pour éviter des fuites mémoire.
    private void OnDestroy()
    {
        // Vérifie que la référence TurnManager est encore valide.
        if (_turnManager != null)
        {
            // Supprime l'abonnement à l'événement d'évaluation laser.
            _turnManager.OnEvaluateLaser.RemoveListener(HandleEvaluateLaser);

            // Supprime l'abonnement à l'événement d'avancement laser.
            _turnManager.OnAdvanceLaser.RemoveListener(HandleAdvanceLaser);
        }
    }

    // -------------------------------------------------------------------------
    // API publique
    // -------------------------------------------------------------------------

    /// <summary>
    /// Vérifie si playerPos est dans les cellules actives du pattern courant.
    /// Retourne true et émet OnPlayerHitByLaser si le joueur est touché.
    /// </summary>
    public bool EvaluatePosition(Vector2Int playerPos)
    {
        // Retourne faux immédiatement si aucun pattern n'est disponible.
        if (!HasValidPatterns)
            return false;

        // Récupère le pattern laser actuellement actif dans la séquence.
        LaserPattern current = GetCurrentPattern();

        // Retourne faux si le pattern ou ses cellules sont invalides.
        if (current == null || current.ActiveCells == null)
            return false;

        // Vérifie si la position joueur correspond à une cellule active.
        bool isHit = current.ActiveCells.Contains(playerPos);

        // Notifie les abonnés quand le joueur est touché par le laser.
        if (isHit)
        {
            OnPlayerHitByLaser.Invoke();
        }

        return isHit;
    }

    /// <summary>
    /// Retourne une copie des cellules actives du pattern courant.
    /// Destiné exclusivement aux systèmes de rendu externes.
    /// </summary>
    public List<Vector2Int> GetCurrentLaserCells()
    {
        // Retourne une copie de la liste mutable pour protéger l'état interne.
        return new List<Vector2Int>(_activeLaserCells);
    }

    /// <summary>
    /// Retourne une copie des cellules du pattern suivant dans la séquence.
    /// Destiné exclusivement aux systèmes de rendu d'indicateur laser.
    /// </summary>
    public List<Vector2Int> GetNextLaserCells()
    {
        // Retourne une liste vide si aucun pattern n'est disponible.
        if (!HasValidPatterns)
            return new List<Vector2Int>();

        // Calcule l'index du prochain pattern en bouclant sur la séquence.
        int nextIndex = (_currentPatternIndex + 1) % _patterns.Count;

        // Récupère le pattern correspondant à l'index suivant calculé.
        LaserPattern next = _patterns[nextIndex];

        // Retourne une liste vide si le pattern suivant est invalide.
        if (next == null || next.ActiveCells == null)
            return new List<Vector2Int>();

        // Retourne une copie pour protéger les données source ScriptableObject.
        return new List<Vector2Int>(next.ActiveCells);
    }

    // -------------------------------------------------------------------------
    // Gestionnaires d'événements privés
    // -------------------------------------------------------------------------

    // Récupère la position joueur et lance l'évaluation laser.
    private void HandleEvaluateLaser()
    {
        // Ignore l'évaluation si aucun pattern n'est disponible.
        if (!HasValidPatterns)
            return;

        // Récupère la position actuelle du joueur depuis la grille.
        Vector2Int playerPosition = _gridManager.GetPlayerPosition();

        // Évalue si le joueur se trouve dans une cellule laser active.
        EvaluatePosition(playerPosition);
    }

    // Avance l'index au prochain pattern en bouclant sur la séquence.
    private void HandleAdvanceLaser()
    {
        // Ignore l'avancement si aucun pattern n'est disponible.
        if (!HasValidPatterns)
            return;

        // Reste sur l'index zéro si un seul pattern existe.
        if (_patterns.Count == 1)
        {
            // Bloque l'index à zéro pour éviter toute dérive.
            _currentPatternIndex = 0;

            // Recharge les cellules actives même avec un seul pattern.
            RebuildActiveCells();
            return;
        }

        // Passe au pattern suivant et reboucle en fin de séquence.
        _currentPatternIndex = (_currentPatternIndex + 1) % _patterns.Count;

        // Charge les cellules du nouveau pattern dans la liste mutable.
        RebuildActiveCells();
    }

    // -------------------------------------------------------------------------
    // Méthodes privées utilitaires
    // -------------------------------------------------------------------------

    // Retourne le pattern laser correspondant à l'index courant.
    private LaserPattern GetCurrentPattern()
    {
        // Retourne null si l'index est hors des limites de la liste.
        if (_currentPatternIndex < 0 || _currentPatternIndex >= _patterns.Count)
            return null;

        return _patterns[_currentPatternIndex];
    }

    // Remplace la liste mutable par les cellules du pattern courant.
    private void RebuildActiveCells()
    {
        // Vide la liste avant de la repeupler depuis le ScriptableObject.
        _activeLaserCells.Clear();

        // Récupère le pattern correspondant à l'index courant.
        LaserPattern current = GetCurrentPattern();

        // Ignore le rechargement si le pattern ou ses cellules sont nuls.
        if (current == null || current.ActiveCells == null)
            return;

        // Copie chaque cellule du ScriptableObject dans la liste mutable.
        foreach (Vector2Int cell in current.ActiveCells)
        {
            // Ajoute la cellule à la liste active du tour courant.
            _activeLaserCells.Add(cell);
        }
    }

    // -------------------------------------------------------------------------
    // API publique — synchronisation avec la grille
    // -------------------------------------------------------------------------

    /// <summary>Retire de la liste active la cellule occupée par le joueur.</summary>
    public void SyncWithGrid(GridManager gridManager)
    {
        // Récupère la position actuelle du joueur depuis la grille.
        Vector2Int playerPos = gridManager.GetPlayerPosition();

        // Retire les cellules laser qui coïncident avec la position joueur.
        _activeLaserCells.RemoveAll(cell =>
            cell.x == playerPos.x && cell.y == playerPos.y);
    }
}
