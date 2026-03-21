using UnityEngine;
using UnityEngine.Events;

// Initialise le turn manager après la grille, avant le renderer.
[DefaultExecutionOrder(-10)]
// Séquence uniquement les événements de tour de jeu.
public class TurnManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Références externes
    // -------------------------------------------------------------------------

    // Référence au gestionnaire de grille pour s'abonner.
    [SerializeField] private GridManager _gridManager;

    // Référence au système laser pour la synchronisation de grille.
    [SerializeField] private LaserSystem _laserSystem;

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

    // Valide les références obligatoires au démarrage du composant.
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

        // Interrompt si la référence LaserSystem est absente.
        if (_laserSystem == null)
        {
            // Lève une exception claire pour signaler le problème.
            throw new MissingReferenceException(
                $"[TurnManager] La référence {nameof(_laserSystem)} n'est pas assignée."
            );
        }
    }

    // Abonne le gestionnaire de tour quand le composant s'active.
    private void OnEnable()
    {
        // Confirme l'abonnement à OnPlayerMoved au démarrage
        Debug.Log("[DIAG] TurnManager.OnEnable() — abonnement OnPlayerMoved");
        // Vérifie la référence avant d'abonner le gestionnaire.
        if (_gridManager != null)
            _gridManager.OnPlayerMoved.AddListener(HandlePlayerMoved);
    }

    // Désabonne proprement pour éviter des fuites mémoire.
    private void OnDisable()
    {
        // Retire l'abonnement si la référence est encore valide.
        if (_gridManager != null)
            _gridManager.OnPlayerMoved.RemoveListener(HandlePlayerMoved);
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
        // Confirme que TurnManager reçoit bien l'événement joueur
        Debug.Log($"[DIAG] TurnManager.HandlePlayerMoved() reçu — gameIsOver={_gameIsOver}");
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
        // Confirme que le traitement du tour commence
        Debug.Log("[DIAG] TurnManager.ProcessTurn() — début du tour");
        // Réinitialise le drapeau de fin en attente du tour.
        _gameOverPending = false;

        // Étape 0 : retire les lasers chevauchant la cellule joueur.
        _laserSystem.SyncWithGrid(_gridManager);

        // Étape 1 : verrouille la grille pendant le traitement du tour.
        _gridManager.SetTurnProcessing(true);

        // Étape 2 : avance le laser au prochain pattern défini.
        OnAdvanceLaser.Invoke();

        // Étape 3 : évalue la collision joueur-laser du nouveau pattern.
        OnEvaluateLaser.Invoke();

        // Étape 4 : demande l'évaluation de la collecte de pièces.
        OnEvaluateCoins.Invoke();

        // Étape 5 : demande la vérification des conditions de fin.
        OnCheckConditions.Invoke();

        // Interrompt le tour si une fin de partie est signalée.
        if (_gameOverPending)
        {
            // Déverrouille la grille en cas de fin de partie.
            _gridManager.SetTurnProcessing(false);

            // Marque la partie comme définitivement et globalement terminée.
            _gameIsOver = true;

            // Notifie tous les abonnés avec le résultat final.
            OnGameOver.Invoke(_pendingIsVictory);
            return;
        }

        // Étape 6 : déverrouille la grille après le traitement du tour.
        _gridManager.SetTurnProcessing(false);

        // Étape 7 : incrémente le minuteur de spawn de pièces.
        OnTickCoinSpawn.Invoke();

        // Confirme que OnTurnProcessed va être déclenché
        Debug.Log("[DIAG] TurnManager.ProcessTurn() — OnTurnProcessed.Invoke()");
        // Étape 8 : notifie la fin du traitement complet de ce tour.
        OnTurnProcessed.Invoke();
    }
}
