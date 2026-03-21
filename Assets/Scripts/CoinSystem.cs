using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Gère le spawn, la durée de vie et la collecte des pièces.
public class CoinSystem : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Références externes
    // -------------------------------------------------------------------------

    // Référence au gestionnaire de tour pour s'abonner.
    [SerializeField] private TurnManager _turnManager;

    // Référence au gestionnaire de grille pour les positions.
    [SerializeField] private GridManager _gridManager;

    // Référence au système laser pour exclure ses cellules.
    [SerializeField] private LaserSystem _laserSystem;

    // -------------------------------------------------------------------------
    // Paramètres configurables
    // -------------------------------------------------------------------------

    // Colonnes de la grille, doit correspondre à GridManager.
    [SerializeField] private int _gridColumns = 5;

    // Lignes de la grille, doit correspondre à GridManager.
    [SerializeField] private int _gridRows = 5;

    // Tours avant expiration de la pièce active courante.
    [SerializeField] private int _coinLifetimeTurns = 3;

    // Tours d'attente avant l'apparition de la suivante.
    [SerializeField] private int _spawnDelayTurns = 2;

    // Pièces à collecter pour déclencher la condition victoire.
    [SerializeField] private int _victoryCollectCount = 3;

    // Tentatives maximum de spawn pour éviter les cellules laser.
    [SerializeField] private int _maxSpawnAttempts = 10;

    // -------------------------------------------------------------------------
    // Événements publics
    // -------------------------------------------------------------------------

    // Déclenché à chaque collecte avec le total accumulé.
    public UnityEvent<int> OnCoinCollected = new UnityEvent<int>();

    // Déclenché quand la condition de victoire est atteinte.
    public UnityEvent OnVictoryConditionMet = new UnityEvent();

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    // Position de la pièce active, nulle si aucune présente.
    private Vector2Int? _currentCoinPos;

    // Tours restants avant expiration de la pièce active.
    private int _coinLifetimeRemaining;

    // Indique si le système attend le délai de respawn.
    private bool _isWaitingToSpawn;

    // Tours restants avant l'apparition de la prochaine pièce.
    private int _spawnDelayRemaining;

    // Nombre total de pièces collectées depuis le début.
    private int _totalCollected;

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
                $"[CoinSystem] La référence {nameof(_turnManager)} n'est pas assignée."
            );
        }

        // Lève une exception si GridManager n'est pas assigné.
        if (_gridManager == null)
        {
            throw new MissingReferenceException(
                $"[CoinSystem] La référence {nameof(_gridManager)} n'est pas assignée."
            );
        }

        // Lève une exception si LaserSystem n'est pas assigné.
        if (_laserSystem == null)
        {
            throw new MissingReferenceException(
                $"[CoinSystem] La référence {nameof(_laserSystem)} n'est pas assignée."
            );
        }

        // Abonne l'évaluation à l'événement dédié du TurnManager.
        _turnManager.OnEvaluateCoins.AddListener(HandleEvaluateCoins);

        // Abonne le tick à l'événement de minuteur du TurnManager.
        _turnManager.OnTickCoinSpawn.AddListener(HandleTickCoinSpawn);
    }

    // Lance le spawn initial de la pièce au démarrage.
    private void Start()
    {
        // Tente de placer la première pièce sur la grille.
        TrySpawnCoin();
    }

    // Désabonne proprement pour éviter des fuites mémoire.
    private void OnDestroy()
    {
        // Vérifie que la référence TurnManager est encore valide.
        if (_turnManager != null)
        {
            // Supprime l'abonnement à l'événement d'évaluation pièces.
            _turnManager.OnEvaluateCoins.RemoveListener(HandleEvaluateCoins);

            // Supprime l'abonnement à l'événement de minuteur spawn.
            _turnManager.OnTickCoinSpawn.RemoveListener(HandleTickCoinSpawn);
        }
    }

    // -------------------------------------------------------------------------
    // API publique
    // -------------------------------------------------------------------------

    /// <summary>
    /// Vérifie si le joueur occupe la cellule de la pièce active.
    /// Collecte la pièce et démarre un nouveau cycle si c'est le cas.
    /// </summary>
    public bool EvaluateCollection(Vector2Int playerPos)
    {
        // Retourne faux immédiatement si aucune pièce n'est active.
        if (_currentCoinPos == null)
            return false;

        // Retourne faux si le joueur n'est pas sur la pièce.
        if (playerPos != _currentCoinPos.Value)
            return false;

        // Collecte la pièce et déclenche le cycle de respawn.
        CollectCoin();
        return true;
    }

    /// <summary>
    /// Retourne la position de la pièce active, ou null si absente.
    /// Destiné exclusivement aux systèmes de rendu externes.
    /// </summary>
    public Vector2Int? GetCurrentCoinPos()
    {
        // Retourne directement la position nullable de la pièce.
        return _currentCoinPos;
    }

    // -------------------------------------------------------------------------
    // Gestionnaires d'événements privés
    // -------------------------------------------------------------------------

    // Récupère la position joueur et évalue la collecte.
    private void HandleEvaluateCoins()
    {
        // Récupère la position actuelle du joueur depuis la grille.
        Vector2Int playerPosition = _gridManager.GetPlayerPosition();

        // Évalue si le joueur est sur la cellule de la pièce.
        EvaluateCollection(playerPosition);
    }

    // Décrémente les minuteurs de durée de vie et de spawn.
    private void HandleTickCoinSpawn()
    {
        // Fait défiler la durée de vie si une pièce est active.
        if (_currentCoinPos.HasValue)
        {
            // Décrémente le compteur de durée de vie de la pièce.
            _coinLifetimeRemaining--;

            // Supprime la pièce si sa durée de vie est épuisée.
            if (_coinLifetimeRemaining <= 0)
            {
                // Retire la pièce et démarre le délai de respawn.
                RemoveCoin();
                StartSpawnDelay();
            }
        }
        // Décrémente le délai si le système attend un respawn.
        else if (_isWaitingToSpawn)
        {
            // Décrémente le compteur de tours avant l'apparition.
            _spawnDelayRemaining--;

            // Tente le spawn quand le délai d'attente est écoulé.
            if (_spawnDelayRemaining <= 0)
            {
                // Désactive l'attente et tente de placer la pièce.
                _isWaitingToSpawn = false;
                TrySpawnCoin();
            }
        }
    }

    // -------------------------------------------------------------------------
    // Logique de collecte privée
    // -------------------------------------------------------------------------

    // Retire, incrémente le total et déclenche les notifications.
    private void CollectCoin()
    {
        // Retire la pièce collectée de la position courante.
        RemoveCoin();

        // Incrémente le nombre total de pièces collectées.
        _totalCollected++;

        // Notifie les abonnés avec le nouveau total accumulé.
        OnCoinCollected.Invoke(_totalCollected);

        // Vérifie si la condition de victoire est maintenant atteinte.
        if (_totalCollected >= _victoryCollectCount)
        {
            // Notifie les abonnés que la victoire est déclenchée.
            OnVictoryConditionMet.Invoke();
            return;
        }

        // Lance le délai de respawn après une collecte réussie.
        StartSpawnDelay();
    }

    // -------------------------------------------------------------------------
    // Logique de spawn privée
    // -------------------------------------------------------------------------

    // Efface la position active et remet la durée à zéro.
    private void RemoveCoin()
    {
        // Annule la position pour signaler l'absence de pièce.
        _currentCoinPos        = null;

        // Remet le compteur de durée de vie de la pièce à zéro.
        _coinLifetimeRemaining = 0;
    }

    // Démarre le délai d'attente avant le prochain spawn.
    private void StartSpawnDelay()
    {
        // Active le drapeau d'attente et assigne le délai configuré.
        _isWaitingToSpawn   = true;
        _spawnDelayRemaining = _spawnDelayTurns;
    }

    // Sélectionne une cellule valide et y place la pièce.
    private void TrySpawnCoin()
    {
        // Récupère toutes les cellules disponibles pour le spawn.
        List<Vector2Int> available = GetAvailableCells();

        // Réactive l'attente si aucune cellule n'est disponible.
        if (available.Count == 0)
        {
            // Réessaie au prochain tick de minuteur de spawn.
            StartSpawnDelay();
            return;
        }

        // Tente de placer la pièce en évitant les cellules laser.
        for (int attempt = 0; attempt < _maxSpawnAttempts; attempt++)
        {
            // Choisit un candidat aléatoire parmi les cellules libres.
            int randomIndex       = Random.Range(0, available.Count);
            Vector2Int candidate  = available[randomIndex];

            // Récupère les cellules laser actives pour la validation.
            List<Vector2Int> laserCells = _laserSystem.GetCurrentLaserCells();

            // Valide que le candidat n'est pas sur une cellule laser.
            if (!laserCells.Contains(candidate))
            {
                // Place la pièce sur la cellule validée et initialise sa durée.
                _currentCoinPos        = candidate;
                _coinLifetimeRemaining = _coinLifetimeTurns;
                return;
            }
        }

        // Ignore silencieusement si toutes les tentatives ont échoué.
        StartSpawnDelay();
    }

    // Construit la liste des cellules valides pour le spawn.
    private List<Vector2Int> GetAvailableCells()
    {
        // Récupère la position actuelle du joueur depuis la grille.
        Vector2Int playerPos = _gridManager.GetPlayerPosition();

        // Récupère les cellules actuellement occupées par le laser.
        List<Vector2Int> laserCells = _laserSystem.GetCurrentLaserCells();

        // Initialise la liste des candidats valides pour le spawn.
        List<Vector2Int> available = new List<Vector2Int>();

        // Parcourt toutes les colonnes de la grille logique.
        for (int col = 0; col < _gridColumns; col++)
        {
            // Parcourt toutes les lignes de la colonne courante.
            for (int row = 0; row < _gridRows; row++)
            {
                Vector2Int candidate = new Vector2Int(col, row);

                // Exclut la cellule actuellement occupée par le joueur.
                if (candidate == playerPos)
                    continue;

                // Exclut les cellules occupées par le laser actif.
                if (laserCells.Contains(candidate))
                    continue;

                // Ajoute la cellule à la liste des candidats retenus.
                available.Add(candidate);
            }
        }

        return available;
    }
}
