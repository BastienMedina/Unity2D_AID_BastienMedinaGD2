using UnityEngine;

public class BarrelProjectile : MonoBehaviour
{
    [SerializeField] private float _speed = 15f;
    [SerializeField] private int _damage = 1;
    [SerializeField] private float _maxRange = 10f;
    [SerializeField] private AudioClip _impactClip;

    private Vector2 _direction;
    private Vector3 _spawnPosition;
    private Rigidbody2D _rigidbody;

    private void Awake() // Initialise le Rigidbody et la position de spawn
    {
        _rigidbody     = GetComponent<Rigidbody2D>();
        _spawnPosition = transform.position;

        if (_rigidbody == null)
            Debug.LogError("[BarrelProjectile] Rigidbody2D manquant.", this);
    }

    public void Initialize(Vector2 direction, float speed, int damage) // Injecte la direction, vitesse et dégâts
    {
        _direction = direction;
        _speed     = speed;
        _damage    = damage;
    }

    private void FixedUpdate() // Déplace le baril et détruit si portée dépassée
    {
        if (_rigidbody == null) return;

        _rigidbody.MovePosition(_rigidbody.position + _direction * (_speed * Time.fixedDeltaTime));

        if (Vector3.Distance(_spawnPosition, _rigidbody.position) >= _maxRange) // Détruit si hors portée
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other) // Détecte les ennemis et murs via trigger
    {
        IEnemyDamageable target = other.GetComponent<IEnemyDamageable>()
                               ?? other.GetComponentInParent<IEnemyDamageable>();

        if (target != null) // Inflige dégâts et se détruit
        {
            target.TakeDamage(_damage);
            AudioManager.Instance?.PlaySFX(_impactClip);
            Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Wall")) // Détruit sur mur
            Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D other) // Détecte les ennemis via collision physique
    {
        IEnemyDamageable target = other.gameObject.GetComponent<IEnemyDamageable>()
                               ?? other.gameObject.GetComponentInParent<IEnemyDamageable>();

        if (target != null) // Inflige dégâts et se détruit
        {
            target.TakeDamage(_damage);
            AudioManager.Instance?.PlaySFX(_impactClip);
            Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Wall")) // Détruit sur mur
            Destroy(gameObject);
    }
}
