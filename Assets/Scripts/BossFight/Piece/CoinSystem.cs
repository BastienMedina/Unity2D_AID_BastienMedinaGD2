using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CoinSystem : MonoBehaviour
{
    [SerializeField] private TurnManager _turnManager;
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private LaserSystem _laserSystem;
    [SerializeField] private int _gridColumns = 5;
    [SerializeField] private int _gridRows = 5;
    [SerializeField] private int _coinLifetimeTurns = 3;
    [SerializeField] private int _spawnDelayTurns = 2;
    [SerializeField] private int _victoryCollectCount = 3;
    [SerializeField] private int _maxSpawnAttempts = 10;
    [SerializeField] private AudioClip _collectCoinClip;
    [SerializeField] private AudioClip _spawnCoinClip;
    [SerializeField] private AudioClip _victoryClip;

    public UnityEvent<int> OnCoinCollected = new UnityEvent<int>();
    public UnityEvent OnVictoryConditionMet = new UnityEvent();

    private Vector2Int? _currentCoinPos;
    private int _coinLifetimeRemaining;
    private bool _isWaitingToSpawn;
    private int _spawnDelayRemaining;
    private int _totalCollected;

    private void Awake() // Valide les refs et s'abonne aux événements de tour
    {
        if (_turnManager == null)
            throw new MissingReferenceException($"[CoinSystem] {nameof(_turnManager)} non assigné.");
        if (_gridManager == null)
            throw new MissingReferenceException($"[CoinSystem] {nameof(_gridManager)} non assigné.");
        if (_laserSystem == null)
            throw new MissingReferenceException($"[CoinSystem] {nameof(_laserSystem)} non assigné.");

        _turnManager.OnEvaluateCoins.AddListener(HandleEvaluateCoins);
        _turnManager.OnTickCoinSpawn.AddListener(HandleTickCoinSpawn);
    }

    private void Start() // Spawn la première pièce au démarrage
    {
        TrySpawnCoin();
    }

    private void OnDestroy() // Désabonne les écouteurs de tour
    {
        if (_turnManager != null)
        {
            _turnManager.OnEvaluateCoins.RemoveListener(HandleEvaluateCoins);
            _turnManager.OnTickCoinSpawn.RemoveListener(HandleTickCoinSpawn);
        }
    }

    public bool EvaluateCollection(Vector2Int playerPos) // Collecte la pièce si le joueur est dessus
    {
        if (_currentCoinPos == null)
            return false;

        if (playerPos != _currentCoinPos.Value)
            return false;

        CollectCoin();
        return true;
    }

    public Vector2Int? GetCurrentCoinPos() => _currentCoinPos; // Retourne la position de la pièce active

    private void HandleEvaluateCoins() // Évalue la collecte depuis la position joueur
    {
        EvaluateCollection(_gridManager.GetPlayerPosition());
    }

    private void HandleTickCoinSpawn() // Décrémente durée de vie ou délai de spawn
    {
        if (_currentCoinPos.HasValue) // Pièce active : décrémente sa durée de vie
        {
            _coinLifetimeRemaining--;
            if (_coinLifetimeRemaining <= 0)
            {
                RemoveCoin();
                StartSpawnDelay();
            }
        }
        else if (_isWaitingToSpawn) // Attente de respawn : décrémente le délai
        {
            _spawnDelayRemaining--;
            if (_spawnDelayRemaining <= 0)
            {
                _isWaitingToSpawn = false;
                TrySpawnCoin();
            }
        }
    }

    private void CollectCoin() // Incrémente le total et vérifie la victoire
    {
        RemoveCoin();
        _totalCollected++;
        AudioManager.Instance?.PlaySFX(_collectCoinClip);
        OnCoinCollected.Invoke(_totalCollected);

        if (_totalCollected >= _victoryCollectCount) // Condition victoire atteinte
        {
            AudioManager.Instance?.PlaySFX(_victoryClip);
            OnVictoryConditionMet.Invoke();
            return;
        }

        StartSpawnDelay();
    }

    private void RemoveCoin() // Efface la position et remet la durée à zéro
    {
        _currentCoinPos        = null;
        _coinLifetimeRemaining = 0;
    }

    private void StartSpawnDelay() // Active l'attente avant le prochain spawn
    {
        _isWaitingToSpawn    = true;
        _spawnDelayRemaining = _spawnDelayTurns;
    }

    private void TrySpawnCoin() // Choisit une cellule valide et place la pièce
    {
        List<Vector2Int> available = GetAvailableCells();

        if (available.Count == 0) // Aucune cellule libre, réessaie plus tard
        {
            StartSpawnDelay();
            return;
        }

        for (int attempt = 0; attempt < _maxSpawnAttempts; attempt++) // Tente d'éviter les cellules laser
        {
            Vector2Int candidate = available[Random.Range(0, available.Count)];
            if (!_laserSystem.GetCurrentLaserCells().Contains(candidate))
            {
                _currentCoinPos        = candidate;
                _coinLifetimeRemaining = _coinLifetimeTurns;
                AudioManager.Instance?.PlaySFX(_spawnCoinClip);
                return;
            }
        }

        StartSpawnDelay(); // Toutes les tentatives ont échoué
    }

    private List<Vector2Int> GetAvailableCells() // Retourne les cellules hors joueur et hors laser
    {
        Vector2Int playerPos = _gridManager.GetPlayerPosition();
        List<Vector2Int> laserCells = _laserSystem.GetCurrentLaserCells();
        List<Vector2Int> available  = new List<Vector2Int>();

        for (int col = 0; col < _gridColumns; col++) // Parcourt toutes les colonnes
        {
            for (int row = 0; row < _gridRows; row++) // Parcourt toutes les lignes
            {
                Vector2Int candidate = new Vector2Int(col, row);
                if (candidate == playerPos) continue; // Exclut la cellule joueur
                if (laserCells.Contains(candidate)) continue; // Exclut les cellules laser
                available.Add(candidate);
            }
        }

        return available;
    }
}
