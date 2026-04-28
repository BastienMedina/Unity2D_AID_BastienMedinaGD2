using UnityEngine;
using UnityEngine.Events;

// Gère la mise en pause et la reprise du jeu via Time.timeScale
public class PauseManager : MonoBehaviour
{
    // Événement déclenché quand le jeu est mis en pause
    [SerializeField] public UnityEvent _onPaused;

    // Événement déclenché quand le jeu reprend après une pause
    [SerializeField] public UnityEvent _onResumed;

    // Son joué lors de la mise en pause
    [SerializeField] private AudioClip _pauseClip;

    // Son joué lors de la reprise du jeu
    [SerializeField] private AudioClip _resumeClip;

    // Indique si le jeu est actuellement en pause
    private bool _isPaused = false;

    /// <summary>Retourne vrai si le jeu est actuellement en pause.</summary>
    public bool IsPaused => _isPaused;

    /// <summary>Bascule entre pause et reprise du jeu.</summary>
    public void TogglePause()
    {
        Debug.Log($"[PauseManager] TogglePause — état actuel _isPaused : {_isPaused}", this);
        if (_isPaused)
            Resume();
        else
            Pause();
    }

    /// <summary>Met le jeu en pause en gelant Time.timeScale à zéro.</summary>
    public void Pause()
    {
        if (_isPaused)
        {
            Debug.Log("[PauseManager] Pause() ignoré — déjà en pause.", this);
            return;
        }

        Time.timeScale = 0f;
        _isPaused = true;

        if (_pauseClip != null)
            AudioManager.Instance?.PlaySFX(_pauseClip);

        Debug.Log($"[PauseManager] Pause() — _onPaused listeners : {_onPaused.GetPersistentEventCount()} persistants. Invocation...", this);
        _onPaused?.Invoke();
        Debug.Log("[PauseManager] Pause() — _onPaused.Invoke() terminé.", this);
    }

    /// <summary>Reprend le jeu en restaurant Time.timeScale à un.</summary>
    public void Resume()
    {
        if (!_isPaused)
        {
            Debug.Log("[PauseManager] Resume() ignoré — pas en pause.", this);
            return;
        }

        Time.timeScale = 1f;
        _isPaused = false;

        if (_resumeClip != null)
            AudioManager.Instance?.PlaySFX(_resumeClip);

        Debug.Log($"[PauseManager] Resume() — _onResumed listeners : {_onResumed.GetPersistentEventCount()} persistants. Invocation...", this);
        _onResumed?.Invoke();
        Debug.Log("[PauseManager] Resume() — _onResumed.Invoke() terminé.", this);
    }

    // Restaure le timeScale si le gestionnaire est détruit en cours de pause
    private void OnDestroy()
    {
        // Garantit que le jeu ne reste pas bloqué si l'objet est supprimé
        Time.timeScale = 1f;
    }
}
