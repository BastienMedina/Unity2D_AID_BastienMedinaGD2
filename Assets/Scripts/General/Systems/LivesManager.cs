using UnityEngine;
using UnityEngine.Events;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance { get; private set; }

    [SerializeField] private int _maxLives = 3;
    [SerializeField] private AudioClip _losLifeClip;
    [SerializeField] private AudioClip _healClip;
    [SerializeField] private AudioClip _deathClip;
    [SerializeField] private float _invincibilityDuration = 0.8f;

    public UnityEvent<int> OnLivesChanged    = new UnityEvent<int>();
    public UnityEvent<int> OnMaxHealthChanged = new UnityEvent<int>();
    public UnityEvent OnDeath                 = new UnityEvent();

    private float _invincibilityTimer = 0f;
    private int   _currentLives;

    private void Update() // Décrémente le timer d'invincibilité chaque frame
    {
        if (_invincibilityTimer > 0f) _invincibilityTimer -= Time.deltaTime;
    }

    private void Awake() // Initialise le singleton et restaure les vies depuis GameProgress
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (GameProgress.Instance != null && GameProgress.Instance.HasPersistedMaxLives)
            _maxLives = GameProgress.Instance.PopMaxLives(); // Restaure le PV max en priorité

        if (GameProgress.Instance != null && GameProgress.Instance.HasPersistedLives)
            _currentLives = Mathf.Min(GameProgress.Instance.PopLives(), _maxLives); // Clamp aux vies max
        else
            _currentLives = _maxLives;

        OnLivesChanged.Invoke(_currentLives);
    }

    private void OnDestroy() // Libère le slot singleton pour la prochaine scène
    {
        if (Instance == this) Instance = null;
    }

    public int GetCurrentLives() => _currentLives; // Expose les vies courantes en lecture

    public void LoseLife() // Décrémente les vies et notifie les abonnés
    {
        if (_currentLives <= 0) return;
        _currentLives--;
        AudioManager.Instance?.PlaySFX(_losLifeClip);
        OnLivesChanged.Invoke(_currentLives);
    }

    public void ResetLives() // Restaure le maximum et notifie
    {
        _currentLives = _maxLives;
        OnLivesChanged.Invoke(_currentLives);
    }

    public void Heal(int amount) // Soigne et notifie si pas au maximum
    {
        if (_currentLives >= _maxLives) return;
        _currentLives = Mathf.Min(_currentLives + amount, _maxLives);
        AudioManager.Instance?.PlaySFX(_healClip);
        OnLivesChanged.Invoke(_currentLives);
    }

    public void TakeDamage() // Inflige un dégât avec fenêtre d'invincibilité
    {
        if (_currentLives <= 0 || _invincibilityTimer > 0f) return;

        _currentLives--;
        _invincibilityTimer = _invincibilityDuration; // Démarre l'invincibilité
        OnLivesChanged.Invoke(_currentLives);

        if (_currentLives <= 0) { AudioManager.Instance?.PlaySFX(_deathClip); OnDeath.Invoke(); }
        else                    { AudioManager.Instance?.PlaySFX(_losLifeClip); }
    }

    public void SetMaxHealth(int newMax) // Met à jour le PV max et notifie les deux événements
    {
        _maxLives     = newMax;
        _currentLives = Mathf.Min(_currentLives, _maxLives);
        OnMaxHealthChanged.Invoke(_maxLives);
        OnLivesChanged.Invoke(_currentLives);
    }

    public int GetMaxLives() => _maxLives; // Expose le PV maximum en lecture
}
