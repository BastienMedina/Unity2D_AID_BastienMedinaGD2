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
        // Choisit la bonne action selon l'état courant de la pause
        if (_isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    /// <summary>Met le jeu en pause en gelant Time.timeScale à zéro.</summary>
    public void Pause()
    {
        // Ignore si le jeu est déjà en pause pour éviter les doublons
        if (_isPaused)
        {
            return;
        }

        // Gèle le temps Unity pour bloquer tous les Update et physique
        Time.timeScale = 0f;

        // Marque l'état interne comme en pause
        _isPaused = true;

        // Joue le son de pause en temps non scalé
        if (_pauseClip != null)
            AudioManager.Instance?.PlaySFX(_pauseClip);

        // Notifie les abonnés de la mise en pause
        _onPaused?.Invoke();
    }

    /// <summary>Reprend le jeu en restaurant Time.timeScale à un.</summary>
    public void Resume()
    {
        // Ignore si le jeu n'est pas en pause pour éviter les doublons
        if (!_isPaused)
        {
            return;
        }

        // Restaure le temps Unity pour reprendre la simulation
        Time.timeScale = 1f;

        // Marque l'état interne comme actif
        _isPaused = false;

        // Joue le son de reprise
        if (_resumeClip != null)
            AudioManager.Instance?.PlaySFX(_resumeClip);

        // Notifie les abonnés de la reprise du jeu
        _onResumed?.Invoke();
    }

    // Restaure le timeScale si le gestionnaire est détruit en cours de pause
    private void OnDestroy()
    {
        // Garantit que le jeu ne reste pas bloqué si l'objet est supprimé
        Time.timeScale = 1f;
    }
}
