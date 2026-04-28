using UnityEngine;

public class SI_VirusBullet : MonoBehaviour
{
    [SerializeField] private float _maxRange = 10f;
    [SerializeField] private AudioClip _impactClip;

    private Vector2 _direction;
    private float _speed;
    private Vector2 _spawnPosition;
    private Rigidbody2D _rigidbody;
    private bool _isInitialized;

    private void Awake() // Récupère le Rigidbody et mémorise la position spawn
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        if (_rigidbody == null)
            Debug.LogError("[VIRUS_BULLET] Rigidbody2D manquant sur SI_VirusBullet");

        _spawnPosition = transform.position;
    }

    public void Initialize(Vector2 direction, float speed) // Injecte direction et vitesse depuis le tireur
    {
        _direction     = direction.normalized;
        _speed         = speed;
        transform.up   = -_direction;
        _isInitialized = true;
    }

    private void FixedUpdate() // Déplace la balle et détruit si portée dépassée
    {
        if (!_isInitialized || _rigidbody == null) return;

        _rigidbody.MovePosition(_rigidbody.position + (_direction * (_speed * Time.fixedDeltaTime)));

        if (Vector2.Distance(_spawnPosition, _rigidbody.position) >= _maxRange) // Détruit si hors portée
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other) // Détecte joueur et murs via trigger
    {
        if (other.CompareTag("Player"))
        {
            LivesManager.Instance?.TakeDamage();
            AudioManager.Instance?.PlaySFX(_impactClip);
            Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Wall")) // Détruit sur mur
            Destroy(gameObject);
    }
}
