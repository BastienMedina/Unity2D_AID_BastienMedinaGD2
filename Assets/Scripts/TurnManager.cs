using UnityEngine;
using UnityEngine.Events;

// Séquence uniquement les événements de tour de jeu.
public class TurnManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Références externes
    // -------------------------------------------------------------------------

    // Référence au gestionnaire de grille pour s'abonner.
    [SerializeField] private GridManager _gridManager;

    // -------------------------------------------------------------------------
    // Événements de séquence — abonnés par les systèmes externes
    // -------------------------------------------------------------------------

    // Déclenché pour évaluer le pattern laser courant.
    public UnityEvent OnEvaluateLaser = new UnityEvent();

    // Déclenché pour évaluer la collecte de pièces.
    public UnityEvent OnEvaluateCoins = new UnityEvent();

    // Déclenché pour vérifier les conditions de fin.
    public UnityEvent OnCheckConditions = new UnityEvent();

    // Déclenché pour avancer le laser au pattern suivant.
    public UnityEvent OnAdvanceLaser = new UnityEvent();

    // Déclenché pour incrémenter le minuteur de spawn pièces.
    public UnityEvent OnTickCoinSpawn = new UnityEvent();

    // -------------------------------------------------------------------------
    // Événements de résultat
    // -------------------------------------------------------------------------

    // Déclenché à la fin de chaque tour complet traité.
    public UnityEvent OnTurnProcessed = new UnityEvent();

    // Déclenché avec le résultat quand la partie se termine.
    public UnityEvent<bool> OnGameOver = new UnityEvent<bool>();

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    // Indique si la partie est actuellement terminée.
    private bool _gameIsOver;

    // Indique qu'une fin de partie a été signalée ce tour.
    private bool _gameOverPending;

    // Résultat de fin transmis par un système externe.
    private bool _pendingIsVictory;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Valide la référence et s'abonne au déplacement joueur.
    private void Awake()
    {
        // Interrompt si la référence GridManager est absente.
        if (_gridManager == null)
        {
            // Lève une exception claire pour signaler le problème.
            throw new MissingReferenceException(
                $"[TurnManager] La référence {nameof(_gridManager)} n'est pas assignée."
            );
        }

        // Abonne le gestionnaire de tour à l'événement joueur.
        _gridManager.OnPlayerMoved.AddListener(HandlePlayerMoved);
    }

    // Désabonne proprement pour éviter des fuites mémoire.
    private void OnDestroy()
    {
        // Vérifie que la référence est encore valide avant désabonnement.
        if (_gridManager != null)
        {
            // Supprime l'abonnement à l'événement de déplacement.
            _gridManager.OnPlayerMoved.RemoveListener(HandlePlayerMoved);
        }
    }

    // -------------------------------------------------------------------------
    // API publique — appelée par les systèmes externes
    // -------------------------------------------------------------------------

    /// <summary>Appelé par un système externe pour signaler une fin de partie.</summary>
    public void NotifyGameOver(bool isVictory)
    {
        // Ignore si la partie est déjà définitivement terminée.
        if (_gameIsOver)
            return;

        // La défaite écrase une victoire déjà enregistrée ce tour.
        if (_gameOverPending && !isVictory && _pendingIsVictory)
        {
            // Remplace la victoire par la défaite, priorité absolue.
            _pendingIsVictory = false;
            return;
        }

        // Ignore si une fin de partie est déjà en attente ce tour.
        if (_gameOverPending)
            return;

        // Enregistre la fin de partie et son résultat reçu.
        _gameOverPending  = true;
        _pendingIsVictory = isVictory;
    }

    // -------------------------------------------------------------------------
    // Gestionnaire d'événement privé
    // -------------------------------------------------------------------------

    // Reçoit le signal de déplacement et déclenche le tour.
    private void HandlePlayerMoved(Vector2Int newPosition)
    {
        // Ignore tout nouveau tour si la partie est terminée.
        if (_gameIsOver)
            return;

        ProcessTurn();
    }

    // -------------------------------------------------------------------------
    // Séquence de tour
    // -------------------------------------------------------------------------

    // Exécute chaque étape du tour dans l'ordre strict.
    private void ProcessTurn()
    {
        // Réinitialise le drapeau de fin en attente du tour.
        _gameOverPending = false;

        // Étape 1 : demande au laser d'évaluer le pattern courant.
        OnEvaluateLaser.Invoke();

        // Étape 2 : demande l'évaluation de la collecte de pièces.
        OnEvaluateCoins.Invoke();

        // Étape 3 : demande la vérification des conditions de fin.
        OnCheckConditions.Invoke();

        // Interrompt le tour si une fin de partie est signalée.
        if (_gameOverPending)
        {
            // Marque la partie comme définitivement et globalement terminée.
            _gameIsOver = true;

            // Notifie tous les abonnés avec le résultat final.
            OnGameOver.Invoke(_pendingIsVictory);
            return;
        }

        // Étape 4 : avance le laser au prochain pattern défini.
        OnAdvanceLaser.Invoke();

        // Étape 5 : incrémente le minuteur de spawn de pièces.
        OnTickCoinSpawn.Invoke();

        // Notifie la fin du traitement complet de ce tour.
        OnTurnProcessed.Invoke();
    }
}
