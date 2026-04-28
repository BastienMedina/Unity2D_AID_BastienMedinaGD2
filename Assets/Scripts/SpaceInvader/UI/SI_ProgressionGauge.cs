using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SI_ProgressionGauge : MonoBehaviour
{
    [SerializeField] private Slider _progressSlider;
    [SerializeField] private float _totalDuration = 120f;
    [SerializeField] private PauseManager _pauseManager;
    [SerializeField] private UnityEvent<float> _onProgressUpdated;
    [SerializeField] private AudioClip _victoryClip;
    [SerializeField] private float _minVictoryDuration = 10f;

    public UnityEvent OnVictory;

    private float _elapsedTime;
    private bool _timerActive = true;
    private bool _finished;
    private float _progressNotifyAccumulator;

    private const float ProgressNotifyInterval = 0.5f;

    private void Awake() // Valide les références et initialise le slider
    {
        if (_totalDuration <= 0f) { Debug.LogWarning("[GAUGE] _totalDuration invalide, corrigé au minimum"); _totalDuration = _minVictoryDuration; }
        if (_progressSlider == null) Debug.LogError("[GAUGE] _progressSlider non assigné.");
        if (_pauseManager   == null) Debug.LogWarning("[GAUGE] PauseManager non assigné — pause ignorée");

        if (_progressSlider != null)
        {
            _progressSlider.minValue    = 0f;
            _progressSlider.maxValue    = 1f;
            _progressSlider.value       = 0f;
            _progressSlider.interactable = false;
        }
    }

    private void Update() // Incrémente le timer, pilote le slider et vérifie la victoire
    {
        if (_finished || !_timerActive) return;
        if (_pauseManager != null && _pauseManager.IsPaused) return;

        _elapsedTime = Mathf.Min(_elapsedTime + Time.deltaTime, _totalDuration);

        if (_progressSlider != null)
            _progressSlider.value = _elapsedTime / _totalDuration;

        _progressNotifyAccumulator += Time.deltaTime;
        if (_progressNotifyAccumulator >= ProgressNotifyInterval) // Notifie à intervalle fixe
        {
            _progressNotifyAccumulator -= ProgressNotifyInterval;
            _onProgressUpdated?.Invoke(GetProgress());
        }

        if (_elapsedTime >= _totalDuration) // Déclenche la victoire si la durée est atteinte
            TriggerVictory();
    }

    private void TriggerVictory() // Verrouille la jauge et déclenche l'événement de victoire
    {
        _finished    = true;
        _timerActive = false;

        if (_progressSlider != null) _progressSlider.value = 1f;

        _onProgressUpdated?.Invoke(1f);
        AudioManager.Instance?.PlaySFX(_victoryClip);
        OnVictory?.Invoke();
    }

    public void PauseTimer()      => _timerActive = false; // Suspend l'incrémentation du timer
    public void ResumeTimer()     => _timerActive = true;  // Reprend l'incrémentation du timer
    public void StopProgression() => _finished    = true;  // Stoppe la jauge sans victoire

    public float GetProgress() // Retourne le ratio normalisé temps écoulé / durée
    {
        if (_totalDuration <= 0f) return 0f;
        return Mathf.Clamp01(_elapsedTime / _totalDuration);
    }
}
