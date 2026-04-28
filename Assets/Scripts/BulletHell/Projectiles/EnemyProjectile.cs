using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private AudioClip _impactClip;

    private Vector2 _direction;
    private float _speed;
    private float _maxRange;
    private LivesManager _livesManager;
    private int _damage;
    private Vector3 _spawnPosition;
    private Rigidbody2D _rigidbody;
    private bool _hasHit = false;

    private void Awake() // Récupère le Rigidbody2D au démarrage
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 direction, float speed, float maxRange, LivesManager livesManager, int damage) // Injecte tous les paramètres de tir
    {
        _direction     = direction;
        _speed         = speed;
        _maxRange      = maxRange;
        _livesManager  = livesManager;
        _damage        = damage;
        _spawnPosition = transform.position;
        transform.up   = -_direction; // Oriente le sprite dans la direction de tir
    }

    private void FixedUpdate() // Déplace le projectile et détruit si portée dépassée
    {
        if (_hasHit) return;

        _rigidbody.MovePosition(_rigidbody.position + _direction * (_speed * Time.fixedDeltaTime));

        if (Vector3.Distance(_spawnPosition, (Vector3)_rigidbody.position) >= _maxRange) // Détruit si hors portée
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other) // Inflige dégâts et se détruit si touche le joueur
    {
        if (_hasHit || !other.CompareTag("Player")) return;

        _hasHit = true;
        _livesManager?.TakeDamage();
        AudioManager.Instance?.PlaySFX(_impactClip);
        Destroy(gameObject);
    }
}
