using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [SerializeField] private AudioClip _pauseClip;
    [SerializeField] private AudioClip _resumeClip;

    private bool _isPaused;
    private bool _isGameOver;

    public bool IsPaused => _isPaused;

    private void Awake() // Initialise le singleton local à la scène
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy() // Restaure le timeScale si l'objet est détruit en pause
    {
        if (Instance == this) { Instance = null; Time.timeScale = 1f; }
    }

    /// <summary>Bascule entre pause et reprise du jeu.</summary>
    public void TogglePause() // Ignore si en état terminal
    {
        if (_isGameOver) return;
        if (_isPaused) Resume(); else Pause();
    }

    /// <summary>Met le jeu en pause.</summary>
    public void Pause() // Bloque le temps et joue le son de pause
    {
        if (_isPaused || _isGameOver) return;
        _isPaused       = true;
        Time.timeScale  = 0f;
        AudioManager.Instance?.PlaySFX(_pauseClip);
    }

    /// <summary>Reprend le jeu après une pause.</summary>
    public void Resume() // Restaure le temps et joue le son de reprise
    {
        if (!_isPaused) return;
        _isPaused       = false;
        Time.timeScale  = 1f;
        AudioManager.Instance?.PlaySFX(_resumeClip);
    }

    /// <summary>Verrouille le PauseManager en état terminal (game over / victoire).</summary>
    public void LockForEndState() // Bloque toute pause ultérieure et gèle le temps
    {
        _isGameOver    = true;
        _isPaused      = false;
        Time.timeScale = 0f;
    }
}
