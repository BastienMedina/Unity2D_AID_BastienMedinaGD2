using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Suit le temps de survie et pilote le slider de progression
public class SI_ProgressionGauge : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Références et durée
    // -------------------------------------------------------------------------

    // Slider UI assigné depuis l'Inspector — doit pointer vers le prefab dans la scène
    [SerializeField] private Slider _progressSlider;

    // Durée totale en secondes à survivre pour déclencher la victoire
    [SerializeField] private float _totalDuration = 120f;

    // Gestionnaire de pause pour suspendre le timer si le jeu est en pause
    [SerializeField] private PauseManager _pauseManager;

    // -------------------------------------------------------------------------
    // Événements publics
    // -------------------------------------------------------------------------

    // Événement déclenché toutes les demi-secondes avec la progression
    [SerializeField] private UnityEvent<float> _onProgressUpdated;

    // Événement déclenché une seule fois à l'atteinte de la victoire
    public UnityEvent OnVictory;

    // -------------------------------------------------------------------------
    // Validation de la durée minimale
    // -------------------------------------------------------------------------

    // Durée minimale acceptable pour la condition de victoire
    [SerializeField] private float _minVictoryDuration = 10f;

    // -------------------------------------------------------------------------
    // État interne du timer
    // -------------------------------------------------------------------------

    // Temps de survie total accumulé depuis le début de la partie
    private float _elapsedTime;

    // Indique si le timer est actif et doit être incrémenté
    private bool _timerActive = true;

    // Indique si la victoire a déjà été déclenchée pour éviter le double
    private bool _finished;

    // Accumulateur interne pour l'intervalle de notification de progression
    private float _progressNotifyAccumulator;

    // Intervalle fixe entre chaque notification de progression en secondes
    private const float ProgressNotifyInterval = 0.5f;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Valide les références et la durée au démarrage
    private void Awake()
    {
        // Corrige la durée si la valeur configurée est nulle ou invalide
        if (_totalDuration <= 0f)
        {
            Debug.LogWarning("[GAUGE] _totalDuration invalide, corrigé au minimum");
            _totalDuration = _minVictoryDuration;
        }

        // Signale si le Slider n'est pas assigné dans l'Inspector
        if (_progressSlider == null)
        {
            Debug.LogError("[GAUGE] _progressSlider non assigné — glisse le prefab Slider_Survival dans l'Inspector.");
        }

        // Signale si le PauseManager est manquant dans l'inspecteur
        if (_pauseManager == null)
        {
            Debug.LogWarning("[GAUGE] PauseManager non assigné — pause ignorée");
        }

        // Initialise le slider à zéro au démarrage
        if (_progressSlider != null)
        {
            _progressSlider.minValue   = 0f;
            _progressSlider.maxValue   = 1f;
            _progressSlider.value      = 0f;
            _progressSlider.interactable = false;
        }
    }

    // Incrémente le timer, pilote le slider et vérifie la victoire
    private void Update()
    {
        // Ignore toute mise à jour si la partie est déjà terminée
        if (_finished)
        {
            return;
        }

        // Ignore si le timer est suspendu manuellement
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

        // Clamp le temps écoulé pour ne pas dépasser la durée totale
        _elapsedTime = Mathf.Min(_elapsedTime, _totalDuration);

        // Avance la valeur du slider en fonction du temps écoulé normalisé
        if (_progressSlider != null)
        {
            // Met à jour le slider avec le ratio temps écoulé sur durée totale
            _progressSlider.value = _elapsedTime / _totalDuration;
        }

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

        // Vérifie si le temps de survie requis est complètement atteint
        if (_elapsedTime >= _totalDuration)
        {
            // Déclenche la séquence de victoire une seule fois
            TriggerVictory();
        }
    }

    // -------------------------------------------------------------------------
    // Victoire
    // -------------------------------------------------------------------------

    // Verrouille la jauge à 1 et déclenche l'événement de victoire
    private void TriggerVictory()
    {
        // Marque la partie comme terminée pour bloquer tout futur incrément
        _finished = true;

        // Désactive le timer pour bloquer tout incrément résiduel post-victoire
        _timerActive = false;

        // Force le slider à sa valeur maximale pour afficher 100 %
        if (_progressSlider != null)
        {
            // Verrouille la valeur du slider à un pour signaler la complétion
            _progressSlider.value = 1f;
        }

        // Notifie les abonnés avec une progression finale à 1
        _onProgressUpdated?.Invoke(1f);

        // Déclenche l'événement de victoire pour les systèmes abonnés
        OnVictory?.Invoke();
    }

    // -------------------------------------------------------------------------
    // API publique
    // -------------------------------------------------------------------------

    /// <summary>Suspend le timer pendant une transition ou à la mort.</summary>
    // Bloque l'incrémentation du timer de survie
    public void PauseTimer()
    {
        // Désactive le flag d'activité pour stopper l'incrémentation
        _timerActive = false;
    }

    /// <summary>Reprend le timer après une transition entre les vagues.</summary>
    // Réactive l'incrémentation du timer de survie
    public void ResumeTimer()
    {
        // Réactive le flag d'activité pour reprendre l'incrémentation
        _timerActive = true;
    }

    /// <summary>Arrête définitivement la progression, par exemple à la mort.</summary>
    // Stoppe la jauge sans déclencher la victoire
    public void StopProgression()
    {
        // Marque la partie comme terminée sans invoquer OnVictory
        _finished = true;
    }

    /// <summary>Retourne la progression normalisée entre 0 et 1.</summary>
    // Calcule le ratio du temps écoulé sur la durée totale
    public float GetProgress()
    {
        // Retourne zéro si la durée totale est invalide ou nulle
        if (_totalDuration <= 0f)
        {
            return 0f;
        }

        // Renvoie le ratio normalisé clampé entre 0 et 1
        return Mathf.Clamp01(_elapsedTime / _totalDuration);
    }
}
