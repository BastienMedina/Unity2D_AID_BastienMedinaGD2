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

        // Signale si le Rigidbody2D est absent du baril
        if (_rigidbody == null)
        {
            Debug.LogError("[BARREL] Rigidbody2D manquant — trigger inopérant sans Rigidbody2D");
        }

        // Vérifie la présence du Rigidbody2D sur le projectile
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Debug.Log($"[COL] Barrel Awake — Rigidbody2D={rb != null} | bodyType={rb?.bodyType}");

        // Vérifie la présence du CircleCollider2D sur le projectile
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        Debug.Log($"[COL] Barrel Awake — CircleCollider2D={col != null} | isTrigger={col?.isTrigger}");

        // Affiche la position de spawn du projectile
        Debug.Log($"[COL] Barrel Awake — spawnPos={transform.position} | direction={_direction}");

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
        // Ignore si le Rigidbody2D est absent du baril
        if (_rigidbody == null)
        {
            return;
        }

        // Affiche la position du projectile chaque seconde environ
        if (Time.frameCount % 50 == 0)
            Debug.Log($"[COL] Barrel FixedUpdate — pos={_rigidbody.position} | velocity={_rigidbody.linearVelocity}");

        // Calcule la prochaine position du projectile
        Vector2 nextPos = _rigidbody.position + _direction * (_speed * Time.fixedDeltaTime);

        // Déplace le projectile via le Rigidbody2D
        _rigidbody.MovePosition(nextPos);

        // Détruit le baril si la distance parcourue dépasse la portée
        if (Vector3.Distance(_spawnPosition, _rigidbody.position) >= _maxRange)
        {
            // Supprime le GameObject quand la portée est dépassée
            Destroy(gameObject);
        }
    }

    // Détecte les ennemis et les murs via le trigger CircleCollider2D
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Confirme que OnTriggerEnter2D est bien déclenché
        Debug.Log($"[COL] OnTriggerEnter2D FIRED — hit={other.gameObject.name}");

        // Affiche le nom de l'objet touché pour diagnostic
        Debug.Log($"[BARREL] OnTriggerEnter2D hit: {other.gameObject.name} layer={other.gameObject.layer}");

        // Vérifie si le collider touché implémente IEnemyDamageable
        IEnemyDamageable target = null;
        other.TryGetComponent(out target);

        // Vérifie aussi sur le parent si composant absent sur l'enfant
        if (target == null)
        {
            target = other.GetComponentInParent<IEnemyDamageable>();
        }

        // Applique les dégâts si la cible est valide
        if (target != null)
        {
            // Inflige les dégâts configurés à la cible
            target.TakeDamage(_damage);

            AudioManager.Instance?.PlaySFX(_impactClip);

            // Détruit le projectile après impact
            Destroy(gameObject);
            return;
        }

        // Détruit le projectile sur les murs également
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            // Supprime le baril à l'impact avec un mur
            Destroy(gameObject);
        }
    }

    // Détecte un contact maintenu avec un objet
    private void OnTriggerStay2D(Collider2D other)
    {
        // Affiche le nom de l'objet en contact prolongé
        Debug.Log($"[COL] OnTriggerStay2D — staying on={other.gameObject.name}");
    }

    // Détecte une collision physique solide
    private void OnCollisionEnter2D(Collision2D other)
    {
        // Affiche le nom de l'objet heurté en collision solide
        Debug.Log($"[COL] OnCollisionEnter2D FIRED — hit={other.gameObject.name}");
    }
}
