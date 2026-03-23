using UnityEngine;

// Déplace le baril, détecte les ennemis et se détruit au contact
public class BarrelProjectile : MonoBehaviour
{
    // Vitesse de déplacement configurable depuis l'inspecteur
    [SerializeField] private float _speed = 5f;

    // Dégâts infligés à l'ennemi touché par le baril
    [SerializeField] private int _damage = 1;

    // Distance maximale avant auto-destruction du baril
    [SerializeField] private float _maxRange = 10f;

    // Direction normalisée transmise par HeroDonkeyKong
    private Vector2 _direction;

    // Position enregistrée à l'apparition du baril
    private Vector3 _spawnPosition;

    // Mémorise la position de départ pour le calcul de portée
    private void Awake()
    {
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

    // Déplace le baril et vérifie la portée maximale chaque frame
    private void Update()
    {
        // Applique le déplacement dans l'espace monde cette frame
        transform.Translate(_direction * (_speed * Time.deltaTime), Space.World);

        // Détruit le baril si la distance parcourue dépasse la portée
        if (Vector3.Distance(_spawnPosition, transform.position) >= _maxRange)
        {
            // Supprime le GameObject quand la portée est dépassée
            Destroy(gameObject);
        }
    }

    // Détecte les ennemis via le trigger CircleCollider2D
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Vérifie si le collider touché implémente IEnemyDamageable
        if (other.TryGetComponent(out IEnemyDamageable enemy))
        {
            // Inflige les dégâts configurés à l'ennemi détecté
            enemy.TakeDamage(_damage);

            // Détruit le baril immédiatement après le contact
            Destroy(gameObject);
        }
    }
}
