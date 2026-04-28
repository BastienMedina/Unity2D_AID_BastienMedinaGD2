using UnityEngine;
using UnityEngine.Events;

public abstract class EnemyBase : MonoBehaviour, IEnemyDamageable
{
    [SerializeField] private int _maxHealth = 2;
    [SerializeField] private UnityEvent _onDamageTaken;
    [SerializeField] public UnityEvent OnDeath;
    [SerializeField] private AudioClip _deathClip;

    private int _currentHealth;
    protected EnemyFeedback _feedback;

    protected virtual void Awake() // Initialise la santé et le feedback visuel
    {
        _currentHealth = _maxHealth;
        _feedback      = GetComponent<EnemyFeedback>();
    }

    public int GetCurrentHealth() => _currentHealth; // Retourne les PV actuels

    public bool IsDead() => _currentHealth <= 0; // Vérifie si les PV sont épuisés

    public void TakeDamage(int amount) // Applique dégâts et déclenche mort si nécessaire
    {
        if (IsDead())
            return;

        _currentHealth -= amount;
        _feedback?.PlayHitFeedback();
        _onDamageTaken?.Invoke();

        if (IsDead()) // Déclenche la séquence de mort
        {
            AudioManager.Instance?.PlaySFX(_deathClip);
            OnDeath?.Invoke();
            HandleDeath();
        }
    }

    protected abstract void HandleDeath();
}
