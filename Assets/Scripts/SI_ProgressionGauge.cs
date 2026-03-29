using UnityEngine;
using UnityEngine.Events;

// Suit le temps de survie et déclenche la victoire à l'échéance
public class SI_ProgressionGauge : MonoBehaviour
{
    // Durée totale en secondes à survivre pour déclencher la victoire
    [SerializeField] private float _victoryDuration = 120f;

    // Durée de la pause du timer lors des transitions entre vagues
    [SerializeField] private float _pauseBetweenWaves = 3f;

    // Référence au gestionnaire de pause pour vérifier l'état du jeu
    [SerializeField] private PauseManager _pauseManager;

    // Événement déclenché toutes les demi-secondes avec la progression
    [SerializeField] private UnityEvent<float> _onProgressUpdated;

    // Événement déclenché une seule fois à l'atteinte de la victoire
    [SerializeField] private UnityEvent _onVictory;

    // Temps de survie total accumulé depuis le début de la partie
    private float _elapsedTime;

    // Indique si le timer est actif et doit être incrémenté
    private bool _timerActive = true;

    // Indique si la victoire a déjà été déclenchée pour éviter le double
    private bool _victoryFired;

    // Accumulateur interne pour l'intervalle de notification de progression
    private float _progressNotifyAccumulator;

    // Intervalle fixe en secondes entre chaque notification de progression
    private const float ProgressNotifyInterval = 0.5f;

    // Durée minimale acceptable en secondes pour la condition de victoire
    [SerializeField] private float _minVictoryDuration = 10f;

    // Vérifie que le PauseManager est assigné dans l'inspecteur
    private void Awake()
    {
        // Clamp _victoryDuration au minimum si la valeur configurée est nulle
        if (_victoryDuration <= 0f)
        {
            // Avertit que la durée configurée est invalide et applique le minimum
            Debug.LogWarning("[GAUGE] _victoryDuration invalide, corrigé au minimum");

            // Force la durée de victoire à la valeur minimale acceptable
            _victoryDuration = _minVictoryDuration;
        }

        // Signale si le PauseManager est manquant dans l'inspecteur
        if (_pauseManager == null)
        {
            Debug.LogError("[GAUGE] PauseManager non assigné dans l'inspecteur");
        }
    }

    // Incrémente le timer et notifie la progression à intervalle régulier
    private void Update()
    {
        // Ignore si la victoire a déjà été déclenchée
        if (_victoryFired)
        {
            return;
        }

        // Ignore si le timer est suspendu manuellement entre les vagues
        if (!_timerActive)
        {
            return;
        }

        // Ignore si le jeu est en pause via le PauseManager
        if (_pauseManager != null && _pauseManager.IsPaused)
        {
            return;
        }

        // Accumule le temps réel écoulé depuis la dernière frame
        _elapsedTime += Time.deltaTime;

        // Clamp le temps écoulé pour ne pas dépasser la durée de victoire
        _elapsedTime = Mathf.Min(_elapsedTime, _victoryDuration);

        // Incrémente l'accumulateur de notification de progression
        _progressNotifyAccumulator += Time.deltaTime;

        // Déclenche la notification si l'intervalle de 0.5s est atteint
        if (_progressNotifyAccumulator >= ProgressNotifyInterval)
        {
            // Réinitialise l'accumulateur au surplus pour rester précis
            _progressNotifyAccumulator -= ProgressNotifyInterval;

            // Notifie les abonnés avec la progression normalisée courante
            _onProgressUpdated?.Invoke(GetProgress());
        }

        // Vérifie si le temps de survie requis est atteint
        if (_elapsedTime >= _victoryDuration)
        {
            // Déclenche la victoire une seule fois
            TriggerVictory();
        }
    }

    // Déclenche l'événement de victoire et bloque les futurs déclenchements
    private void TriggerVictory()
    {
        // Marque la victoire comme déclenchée pour éviter les doublons
        _victoryFired = true;

        // Arrête le timer pour bloquer tout incrément résiduel post-victoire
        _timerActive = false;

        // Notifie les abonnés avec une progression finale à 1
        _onProgressUpdated?.Invoke(1f);

        // Notifie les abonnés que la condition de victoire est atteinte
        _onVictory?.Invoke();
    }

    /// <summary>Suspend le timer pendant la transition entre deux vagues.</summary>
    // Bloque l'incrémentation du timer de survie
    public void PauseTimer()
    {
        // Désactive le flag d'activité pour stopper l'incrémentation
        _timerActive = false;
    }

    /// <summary>Reprend le timer après la transition entre deux vagues.</summary>
    // Réactive l'incrémentation du timer de survie
    public void ResumeTimer()
    {
        // Réactive le flag d'activité pour reprendre l'incrémentation
        _timerActive = true;
    }

    /// <summary>Retourne la progression normalisée entre 0 et 1.</summary>
    // Calcule le ratio du temps écoulé sur la durée de victoire
    public float GetProgress()
    {
        // Retourne zéro si la durée de victoire est invalide ou nulle
        if (_victoryDuration <= 0f)
        {
            return 0f;
        }

        // Renvoie le ratio normalisé clampé entre 0 et 1
        return Mathf.Clamp01(_elapsedTime / _victoryDuration);
    }
}
