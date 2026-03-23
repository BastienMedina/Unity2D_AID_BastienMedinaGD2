using UnityEngine;

// Se déplace vers le joueur, attaque au contact et se despawn
public class EnemyNetworkUnit : EnemyBase
{
    // Référence au Transform du joueur pour le suivi de position
    private Transform _playerTransform;

    // Référence au gestionnaire de vies pour infliger des dégâts
    private LivesManager _livesManager;

    // Référence au spawner pour notifier le retrait de cette unité
    private EnemyNetworkSpawner _spawner;

    // Vitesse de déplacement vers le joueur en unités par seconde
    private float _speed;

    // Dégâts infligés au joueur lors d'une collision directe
    private int _damage;

    // Distance maximale avant que l'unité rentre dans le réseau
    private float _despawnRadius;

    // Position d'origine enregistrée pour le calcul de distance
    private Vector3 _spawnPosition;

    // Initialise l'unité avec toutes les données du spawner
    public void Initialize(Transform playerTransform, LivesManager livesManager, EnemyNetworkSpawner spawner, float speed, int damage, float despawnRadius)
    {
        // Stocke la référence au Transform du joueur
        _playerTransform = playerTransform;

        // Stocke la référence au gestionnaire de vies du joueur
        _livesManager = livesManager;

        // Stocke la référence au spawner pour le callback de retrait
        _spawner = spawner;

        // Stocke la vitesse de déplacement reçue du spawner
        _speed = speed;

        // Stocke les dégâts à infliger au joueur au contact
        _damage = damage;

        // Stocke le rayon de despawn transmis par le spawner
        _despawnRadius = despawnRadius;

        // Mémorise la position d'origine pour le calcul de portée
        _spawnPosition = transform.position;
    }

    // Déplace l'unité vers le joueur et vérifie le rayon de despawn
    private void Update()
    {
        // Stoppe tout mouvement si l'unité est morte
        if (IsDead())
        {
            return;
        }

        // Ignore la logique si le joueur n'est pas référencé
        if (_playerTransform == null)
        {
            return;
        }

        // Déplace l'unité vers la position actuelle du joueur
        transform.position = Vector3.MoveTowards(
            transform.position,
            _playerTransform.position,
            _speed * Time.deltaTime
        );

        // Despawn l'unité si elle dépasse le rayon de la salle
        if (Vector3.Distance(_spawnPosition, transform.position) >= _despawnRadius)
        {
            // Notifie le spawner que cette unité quitte la salle
            Despawn();
        }
    }

    // Inflige des dégâts au joueur lors d'une collision directe
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Vérifie que la collision est bien avec le joueur
        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        // Retire une vie au joueur via le gestionnaire de vies
        _livesManager.TakeDamage();
    }

    // Détruit l'unité et notifie le spawner d'une sortie de salle
    private void Despawn()
    {
        // Prévient le spawner pour libérer un slot d'unité active
        _spawner.NotifyUnitRemoved();

        // Supprime le GameObject de la scène immédiatement
        Destroy(gameObject);
    }

    // Désactive l'unité et notifie le spawner à la mort
    protected override void HandleDeath()
    {
        // Prévient le spawner pour libérer un slot d'unité active
        _spawner.NotifyUnitRemoved();

        // Désactive le GameObject pour le retirer de la scène
        gameObject.SetActive(false);
    }
}
