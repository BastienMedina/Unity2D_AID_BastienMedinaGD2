using UnityEngine;

/// <summary>
/// Gère l'état de pause global via Time.timeScale.
/// Expose IsPaused et les méthodes Pause / Resume / TogglePause.
/// À placer sur un GameObject dans chaque scène de jeu.
/// </summary>
public class PauseManager : MonoBehaviour
{
    // Instance statique locale à la scène — résolue par PauseButtonController et PauseMenuController
    public static PauseManager Instance { get; private set; }

    // Son joué lors de la mise en pause
    [SerializeField] private AudioClip _pauseClip;

    // Son joué lors de la reprise du jeu
    [SerializeField] private AudioClip _resumeClip;

    // Indique si le jeu est actuellement en pause
    private bool _isPaused;

    // Indique si le jeu est en état terminal (game over ou victoire) — bloque la pause
    private bool _isGameOver;

    /// <summary>Vrai si le jeu est actuellement en pause.</summary>
    public bool IsPaused => _isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        // Garantit que le jeu ne reste pas bloqué si l'objet est supprimé pendant la pause
        if (Instance == this)
        {
            Instance = null;
            Time.timeScale = 1f;
        }
    }

    // -------------------------------------------------------------------------
    // API publique
    // -------------------------------------------------------------------------

    /// <summary>Bascule entre pause et reprise du jeu.</summary>
    public void TogglePause()
    {
        if (_isGameOver)
            return;

        if (_isPaused)
            Resume();
        else
            Pause();
    }

    /// <summary>Met le jeu en pause.</summary>
    public void Pause()
    {
        if (_isPaused || _isGameOver)
            return;

        _isPaused = true;
        Time.timeScale = 0f;
        AudioManager.Instance?.PlaySFX(_pauseClip);
    }

    /// <summary>Reprend le jeu après une pause.</summary>
    public void Resume()
    {
        if (!_isPaused)
            return;

        _isPaused = false;
        Time.timeScale = 1f;
        AudioManager.Instance?.PlaySFX(_resumeClip);
    }

    /// <summary>
    /// Verrouille le PauseManager en état terminal (game over / victoire).
    /// Empêche toute pause ultérieure et restaure le timeScale si nécessaire.
    /// Appelé par GameOverMenuController et VictoryMenuController.
    /// </summary>
    public void LockForEndState()
    {
        _isGameOver = true;

        // Si le jeu était en pause au moment du game over, on remet timeScale à 0
        // car c'est l'écran de fin qui gèle maintenant, pas le menu pause
        _isPaused = false;
        Time.timeScale = 0f;
    }
}
