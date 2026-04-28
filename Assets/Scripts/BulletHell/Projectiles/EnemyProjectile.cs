using UnityEngine;

// Déplace le projectile ennemi via Rigidbody2D et inflige des dégâts au joueur au contact.
// Requiert un Rigidbody2D (Kinematic, Continuous) et un Collider2D (isTrigger) sur le prefab.
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    // Direction normalisée transmise par EnemyShooter à l'init
    private Vector2 _direction;

    // Vitesse de déplacement du projectile en unités par seconde
    private float _speed;

    // Distance maximale avant auto-destruction du projectile
    private float _maxRange;

    // Référence au gestionnaire de vies pour toucher le joueur
    private LivesManager _livesManager;

    // Dégâts infligés au joueur lors du contact avec le projectile
    private int _damage;

    // Position enregistrée à l'instanciation pour le calcul de portée
    private Vector3 _spawnPosition;

    // Son joué à l'impact du projectile sur le joueur
    [SerializeField] private AudioClip _impactClip;

    // Rigidbody2D utilisé pour le déplacement physique sans tunneling
    private Rigidbody2D _rigidbody;

    // Flag interne pour ne déclencher l'impact qu'une seule fois
    private bool _hasHit = false;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    // -------------------------------------------------------------------------
    // Initialisation
    // -------------------------------------------------------------------------

    /// <summary>Initialise le projectile avec tous ses paramètres de tir.</summary>
    public void Initialize(Vector2 direction, float speed, float maxRange, LivesManager livesManager, int damage)
    {
        _direction    = direction;
        _speed        = speed;
        _maxRange     = maxRange;
        _livesManager = livesManager;
        _damage       = damage;
        _spawnPosition = transform.position;
    }

    // -------------------------------------------------------------------------
    // Déplacement physique
    // -------------------------------------------------------------------------

    // Déplace le projectile via Rigidbody2D.MovePosition pour éviter le tunneling
    private void FixedUpdate()
    {
        if (_hasHit) return;

        Vector2 nextPos = _rigidbody.position + _direction * (_speed * Time.fixedDeltaTime);
        _rigidbody.MovePosition(nextPos);

        // Détruit le projectile si la portée maximale est dépassée
        if (Vector3.Distance(_spawnPosition, (Vector3)_rigidbody.position) >= _maxRange)
            Destroy(gameObject);
    }

    // -------------------------------------------------------------------------
    // Détection d'impact
    // -------------------------------------------------------------------------

    // Détecte la collision avec le joueur via le trigger du collider
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Garantit qu'un seul impact est traité même si plusieurs triggers se déclenchent
        if (_hasHit) return;

        if (!other.CompareTag("Player")) return;

        _hasHit = true;

        _livesManager?.TakeDamage();
        AudioManager.Instance?.PlaySFX(_impactClip);
        Destroy(gameObject);
    }
}
