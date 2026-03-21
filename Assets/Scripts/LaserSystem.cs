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
        // Retourne une liste vide si aucun pattern n'est disponible.
        if (!HasValidPatterns)
            return new List<Vector2Int>();

        // Récupère le pattern laser actuellement actif dans la séquence.
        LaserPattern current = GetCurrentPattern();

        // Retourne une liste vide si le pattern courant est nul.
        if (current == null || current.ActiveCells == null)
            return new List<Vector2Int>();

        // Retourne une copie pour protéger les données source ScriptableObject.
        return new List<Vector2Int>(current.ActiveCells);
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
            return;
        }

        // Passe au pattern suivant et reboucle en fin de séquence.
        _currentPatternIndex = (_currentPatternIndex + 1) % _patterns.Count;
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
}
