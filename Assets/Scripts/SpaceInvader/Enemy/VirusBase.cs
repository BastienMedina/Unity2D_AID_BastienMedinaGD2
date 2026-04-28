using UnityEngine;
using UnityEngine.Events;

public abstract class VirusBase : MonoBehaviour, IVirusDamageable
{
    [SerializeField] protected int _maxHealth = 1;
    [SerializeField] private float _moveSpeed = 0.5f;
    [SerializeField] private int _scoreValue = 10;
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private UnityEvent<int> _onDamageTaken;
    [SerializeField] private UnityEvent _onDeath;
    [SerializeField] private AudioClip _deathClip;

    public event System.Action OnDeathEvent;

    private int _currentHealth;
    [System.NonSerialized] protected Rigidbody2D _rigidbody;

    protected virtual void Awake() // Initialise la santé et récupère le Rigidbody2D
    {
        _currentHealth = GetInitialHealth();
        _rigidbody     = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() // Déplace le virus vers le bas chaque frame physique
    {
        if (OverridesMovement()) return; // Délègue si sous-classe gère le mouvement

        if (_rigidbody != null)
            _rigidbody.linearVelocity = Vector2.down * _moveSpeed;
        else
            transform.position += Vector3.down * (_moveSpeed * Time.fixedDeltaTime); // Repli sans Rigidbody
    }

    protected virtual bool OverridesMovement() => false; // Retourne vrai si sous-classe gère le mouvement

    protected virtual int GetInitialHealth() => _maxHealth; // Retourne la santé initiale de la sous-classe

    public void TakeDamage(int amount) // Applique les dégâts et déclenche la mort si nécessaire
    {
        if (IsDead()) return;

        _currentHealth -= amount;
        _onDamageTaken?.Invoke(_currentHealth);

        if (IsDead()) // Déclenche la séquence de mort
        {
            AudioManager.Instance?.PlaySFX(_deathClip);
            SpawnExplosion();
            _onDeath?.Invoke();
            OnDeathEvent?.Invoke();
            HandleDeath();
        }
    }

    private void OnTriggerEnter2D(Collider2D other) // Inflige dégâts au serveur au contact
    {
        if (other.CompareTag("Server"))
        {
            other.GetComponent<SI_ServerHealth>()?.TakeDamage(1);
            Destroy(gameObject);
        }
    }

    public bool IsDead() => _currentHealth <= 0; // Retourne vrai si plus de points de vie

    public int GetCurrentHealth() => _currentHealth; // Expose la santé courante en lecture seule

    public int GetScoreValue() => _scoreValue; // Retourne les points de score du virus

    protected abstract void HandleDeath(); // Implémentée par chaque sous-classe à la mort

    private void SpawnExplosion() // Instancie l'explosion à la position courante
    {
        if (_explosionPrefab == null) return;
        Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
    }
}
