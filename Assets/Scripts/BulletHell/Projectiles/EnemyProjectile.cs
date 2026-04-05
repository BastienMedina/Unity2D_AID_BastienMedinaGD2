using UnityEngine;

// Déplace le projectile ennemi et inflige des dégâts au joueur
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

    // Initialise le projectile avec tous ses paramètres de tir
    public void Initialize(Vector2 direction, float speed, float maxRange, LivesManager livesManager, int damage)
    {
        // Stocke la direction normalisée reçue depuis EnemyShooter
        _direction = direction;

        // Stocke la vitesse reçue depuis EnemyShooter
        _speed = speed;

        // Stocke la portée maximale reçue depuis EnemyShooter
        _maxRange = maxRange;

        // Stocke la référence au gestionnaire de vies du joueur
        _livesManager = livesManager;

        // Stocke les dégâts reçus depuis EnemyShooter
        _damage = damage;

        // Mémorise la position de spawn pour le calcul de distance
        _spawnPosition = transform.position;
    }

    // Déplace le projectile et vérifie la portée maximale chaque frame
    private void Update()
    {
        // Applique le déplacement dans l'espace monde cette frame
        transform.Translate(_direction * (_speed * Time.deltaTime), Space.World);

        // Détruit le projectile si la portée maximale est dépassée
        if (Vector3.Distance(_spawnPosition, transform.position) >= _maxRange)
        {
            // Supprime le projectile hors de portée de la scène
            Destroy(gameObject);
        }
    }

    // Détecte la collision avec le joueur via le trigger du collider
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Vérifie si l'objet touché est bien le joueur via son tag
        if (other.CompareTag("Player"))
        {
            // Inflige les dégâts au joueur via le gestionnaire de vies
            _livesManager.TakeDamage();

            // Détruit le projectile immédiatement après le contact
            Destroy(gameObject);
        }
    }
}
