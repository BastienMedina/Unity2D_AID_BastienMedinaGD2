using UnityEngine;

public class SI_PlayerBullet : MonoBehaviour
{
    [SerializeField] private float _speed = 12f;
    [SerializeField] private int _damage = 1;
    [SerializeField] private float _maxRange = 12f;
    [SerializeField] private AudioClip _impactClip;

    private Rigidbody2D _rigidbody;
    private Vector2 _spawnPosition;

    private void Awake() // Initialise le Rigidbody et mémorise la position spawn
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        if (_rigidbody == null)
            Debug.LogError("[BULLET] SI_PlayerBullet — Rigidbody2D manquant sur la balle");

        _spawnPosition = transform.position;
        transform.up   = -Vector2.up;
    }

    private void FixedUpdate() // Déplace la balle et détruit si portée dépassée
    {
        if (_rigidbody == null) return;

        Vector2 nextPosition = _rigidbody.position + (Vector2.up * (_speed * Time.fixedDeltaTime));
        _rigidbody.MovePosition(nextPosition);

        if (Vector2.Distance(_spawnPosition, _rigidbody.position) >= _maxRange) // Détruit si hors portée
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other) // Détecte les virus et murs via trigger
    {
        IVirusDamageable target = null;
        other.TryGetComponent(out target);

        if (target == null)
            target = other.GetComponentInParent<IVirusDamageable>();

        if (target != null) // Inflige dégâts et détruit sur impact virus
        {
            target.TakeDamage(_damage);
            AudioManager.Instance?.PlaySFX(_impactClip);
            Destroy(gameObject);
            return;
        }

        bool hitVirusByTag = other.CompareTag("Virus") // Fallback par tag Virus
            || (other.transform.parent != null && other.transform.parent.CompareTag("Virus"));

        if (hitVirusByTag) { Destroy(gameObject); return; }

        if (other.gameObject.layer == LayerMask.NameToLayer("Wall")) // Détruit sur mur
            Destroy(gameObject);
    }
}
