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

    // Index du pattern utilisé au tour précédent (exclus de la prochaine sélection).
    private int _previousPatternIndex = -1;

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
        _previousPatternIndex = -1;

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
    /// Injecte un pool de patterns générés par code avant le démarrage du jeu.
    /// Appelé par LaserPatternGenerator dans son propre Awake, avant celui-ci.
    /// </summary>
    public void InjectPatterns(List<LaserPattern> patterns)
    {
        _patterns = patterns;
    }

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
    /// Retourne les cellules du pattern qui sera actif au prochain tour.
    /// Utilisé par LaserIndicatorRenderer pour prévenir le joueur.
    /// Le prochain pattern est le suivant dans la séquence après le courant,
    /// en excluant le précédent selon la même logique que HandleAdvanceLaser.
    /// </summary>
    public List<Vector2Int> GetNextLaserCells()
    {
        if (!HasValidPatterns || _patterns.Count < 2)
            return new List<Vector2Int>();

        // Duplique la logique de sélection d'HandleAdvanceLaser pour prédire le prochain index.
        List<int> candidates = new List<int>(_patterns.Count);
        for (int i = 0; i < _patterns.Count; i++)
        {
            if (i == _currentPatternIndex)  continue;
            if (i == _previousPatternIndex) continue;
            candidates.Add(i);
        }

        // Si tous exclus (cas 2 patterns), autorise au moins le précédent.
        if (candidates.Count == 0)
        {
            for (int i = 0; i < _patterns.Count; i++)
            {
                if (i != _currentPatternIndex)
                    candidates.Add(i);
            }
        }

        // Retourne les cellules du premier candidat valide comme prédiction.
        // C'est une estimation : l'index réel sera choisi aléatoirement parmi les candidats.
        LaserPattern next = _patterns[candidates[0]];
        if (next == null || next.ActiveCells == null)
            return new List<Vector2Int>();

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

    // Avance vers un pattern aléatoire en excluant le courant et le précédent.
    private void HandleAdvanceLaser()
    {
        // Ignore l'avancement si aucun pattern n'est disponible.
        if (!HasValidPatterns)
            return;

        // Avec un seul pattern, impossible d'alterner — on reste en place.
        if (_patterns.Count == 1)
        {
            _currentPatternIndex = 0;
            RebuildActiveCells();
            return;
        }

        // Construit la liste des candidats en excluant le courant et le précédent
        List<int> candidates = new List<int>(_patterns.Count);
        for (int i = 0; i < _patterns.Count; i++)
        {
            if (i == _currentPatternIndex) continue;
            if (i == _previousPatternIndex) continue;
            candidates.Add(i);
        }

        // Si tous les patterns sont exclus (2 patterns), autorise au moins le précédent
        if (candidates.Count == 0)
        {
            for (int i = 0; i < _patterns.Count; i++)
            {
                if (i != _currentPatternIndex)
                    candidates.Add(i);
            }
        }

        // Pioche un index aléatoire parmi les candidats valides
        int pickedIndex = candidates[Random.Range(0, candidates.Count)];

        // Mémorise le courant comme précédent avant de changer
        _previousPatternIndex = _currentPatternIndex;
        _currentPatternIndex = pickedIndex;

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
