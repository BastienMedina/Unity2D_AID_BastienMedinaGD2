using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(-10)]
public class TurnManager : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;

    public UnityEvent OnEvaluateLaser    = new UnityEvent();
    public UnityEvent OnEvaluateCoins    = new UnityEvent();
    public UnityEvent OnCheckConditions  = new UnityEvent();
    public UnityEvent OnAdvanceLaser     = new UnityEvent();
    public UnityEvent OnTickCoinSpawn    = new UnityEvent();
    public UnityEvent OnTurnProcessed    = new UnityEvent();
    public UnityEvent<bool> OnGameOver   = new UnityEvent<bool>();

    private bool _gameIsOver;
    private bool _gameOverPending;
    private bool _pendingIsVictory;

    private void Awake() // Vérifie que GridManager est assigné
    {
        if (_gridManager == null)
            throw new MissingReferenceException($"[TurnManager] {nameof(_gridManager)} non assigné.");
    }

    private void OnEnable() // S'abonne au déplacement joueur
    {
        if (_gridManager != null)
            _gridManager.OnPlayerMoved.AddListener(HandlePlayerMoved);
    }

    private void OnDisable() // Désabonne le gestionnaire de déplacement
    {
        if (_gridManager != null)
            _gridManager.OnPlayerMoved.RemoveListener(HandlePlayerMoved);
    }

    public void NotifyGameOver(bool isVictory) // Enregistre la fin de partie ce tour
    {
        if (_gameIsOver)
            return;

        if (_gameOverPending && !isVictory && _pendingIsVictory) // La défaite écrase la victoire
        {
            _pendingIsVictory = false;
            return;
        }

        if (_gameOverPending)
            return;

        _gameOverPending  = true;
        _pendingIsVictory = isVictory;
    }

    private void HandlePlayerMoved(Vector2Int newPosition) // Déclenche un tour à chaque déplacement
    {
        if (_gameIsOver)
            return;

        ProcessTurn();
    }

    private void ProcessTurn() // Exécute toutes les étapes du tour dans l'ordre
    {
        _gameOverPending = false;
        _gridManager.SetTurnProcessing(true); // Verrouille les entrées pendant le tour

        OnAdvanceLaser.Invoke();    // Étape 1 : avance le laser
        OnEvaluateLaser.Invoke();   // Étape 2 : évalue la collision laser
        OnEvaluateCoins.Invoke();   // Étape 3 : évalue la collecte pièce
        OnCheckConditions.Invoke(); // Étape 4 : vérifie les conditions de fin

        if (_gameOverPending) // Fin de partie signalée ce tour
        {
            _gridManager.SetTurnProcessing(false);
            _gameIsOver = true;
            OnGameOver.Invoke(_pendingIsVictory);
            return;
        }

        _gridManager.SetTurnProcessing(false); // Déverrouille les entrées
        OnTickCoinSpawn.Invoke();              // Étape 5 : tick spawn pièce
        OnTurnProcessed.Invoke();              // Étape 6 : fin de tour
    }
}
