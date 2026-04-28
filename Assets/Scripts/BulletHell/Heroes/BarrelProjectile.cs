using UnityEngine;

// Déplace le baril, détecte les ennemis et se détruit au contact
public class BarrelProjectile : MonoBehaviour
{
    // Vitesse de déplacement configurable depuis l'inspecteur
    [SerializeField] private float _speed = 15f;

    // Dégâts infligés à l'ennemi touché par le baril
    [SerializeField] private int _damage = 1;

    // Distance maximale avant auto-destruction du baril
    [SerializeField] private float _maxRange = 10f;

    // Son joué à l'impact du baril sur un ennemi
    [SerializeField] private AudioClip _impactClip;

    // Direction normalisée transmise par HeroDonkeyKong
    private Vector2 _direction;

    // Position enregistrée à l'apparition du baril
    private Vector3 _spawnPosition;

    // Référence au Rigidbody2D du projectile
    private Rigidbody2D _rigidbody;

    // Initialise les références et mémorise la position de départ
    private void Awake()
    {
        // Récupère le Rigidbody2D sur ce GameObject
        _rigidbody = GetComponent<Rigidbody2D>();

        if (_rigidbody == null)
            Debug.LogError("[BarrelProjectile] Rigidbody2D manquant sur le prefab.", this);

        // Enregistre la position monde au moment de l'instanciation
        _spawnPosition = transform.position;
    }

    // Initialise le baril avec direction, vitesse et dégâts
    public void Initialize(Vector2 direction, float speed, int damage)
    {
        // Stocke la direction normalisée reçue depuis HeroDonkeyKong
        _direction = direction;

        // Remplace la vitesse par défaut par celle du lanceur
        _speed = speed;

        // Remplace les dégâts par défaut par ceux du lanceur
        _damage = damage;
    }

    // Déplace le baril via Rigidbody2D et vérifie la portée en physique
    private void FixedUpdate()
    {
        if (_rigidbody == null) return;

        Vector2 nextPos = _rigidbody.position + _direction * (_speed * Time.fixedDeltaTime);
        _rigidbody.MovePosition(nextPos);

        if (Vector3.Distance(_spawnPosition, _rigidbody.position) >= _maxRange)
            Destroy(gameObject);
    }

    // Détecte les ennemis et les murs via le trigger CircleCollider2D
    private void OnTriggerEnter2D(Collider2D other)
    {
        IEnemyDamageable target = null;
        other.TryGetComponent(out target);

        // Vérifie aussi sur le parent si le composant est absent sur l'enfant direct
        if (target == null)
            target = other.GetComponentInParent<IEnemyDamageable>();

        if (target != null)
        {
            target.TakeDamage(_damage);
            AudioManager.Instance?.PlaySFX(_impactClip);
            Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
            Destroy(gameObject);
    }

    // Détecte une collision physique solide (si le collider n'est pas trigger)
    private void OnCollisionEnter2D(Collision2D other)
    {
        IEnemyDamageable target = other.gameObject.GetComponent<IEnemyDamageable>();
        if (target == null)
            target = other.gameObject.GetComponentInParent<IEnemyDamageable>();

        if (target != null)
        {
            target.TakeDamage(_damage);
            AudioManager.Instance?.PlaySFX(_impactClip);
            Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
            Destroy(gameObject);
    }
}
