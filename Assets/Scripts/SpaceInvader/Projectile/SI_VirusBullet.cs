using UnityEngine;

// Déplace la balle virus et détecte les impacts joueur et murs
public class SI_VirusBullet : MonoBehaviour
{
    // Distance maximale parcourue avant auto-destruction de la balle
    [SerializeField] private float _maxRange = 10f;

    // Direction normalisée transmise par VirusShooter à l'initialisation
    private Vector2 _direction;

    // Vitesse de déplacement en unités par seconde, transmise à l'init
    private float _speed;

    // Position enregistrée au spawn pour le calcul de distance parcourue
    private Vector2 _spawnPosition;

    // Rigidbody2D cinématique utilisé pour déplacer la balle via MovePosition
    private Rigidbody2D _rigidbody;

    // Indique si la balle a été initialisée avant de se déplacer
    private bool _isInitialized;

    // Son joué à l'impact de la balle virus sur le joueur
    [SerializeField] private AudioClip _impactClip;

    // Récupère le Rigidbody2D et mémorise la position de spawn
    private void Awake()
    {
        // Récupère le Rigidbody2D requis pour MovePosition
        _rigidbody = GetComponent<Rigidbody2D>();

        // Signale si le Rigidbody2D est manquant sur la balle virus
        if (_rigidbody == null)
        {
            Debug.LogError("[VIRUS_BULLET] Rigidbody2D manquant sur SI_VirusBullet");
        }

        // Mémorise la position monde au moment de l'instanciation
        _spawnPosition = transform.position;
    }

    /// <summary>Initialise la direction et la vitesse de la balle depuis VirusShooter.</summary>
    // Reçoit les paramètres de vol depuis le virus tireur
    public void Initialize(Vector2 direction, float speed)
    {
        // Stocke la direction normalisée reçue depuis VirusShooter
        _direction = direction.normalized;

        // Stocke la vitesse de déplacement reçue depuis VirusShooter
        _speed = speed;

        // Oriente la roquette dans le sens de la direction de tir
        transform.up = -_direction;

        // Marque la balle comme prête à se déplacer en FixedUpdate
        _isInitialized = true;
    }

    // Déplace la balle et vérifie la portée maximale chaque frame physique
    private void FixedUpdate()
    {
        // Abandonne si la balle n'a pas encore été initialisée
        if (!_isInitialized || _rigidbody == null)
        {
            return;
        }

        // Calcule la prochaine position selon direction et vitesse
        Vector2 nextPosition = _rigidbody.position + (_direction * (_speed * Time.fixedDeltaTime));

        // Déplace la balle via MovePosition compatible Kinematic et Dynamic
        _rigidbody.MovePosition(nextPosition);

        // Détruit la balle si la distance parcourue dépasse la portée maximale
        if (Vector2.Distance(_spawnPosition, _rigidbody.position) >= _maxRange)
        {
            // Supprime la balle hors de portée pour économiser les ressources
            Destroy(gameObject);
        }
    }

    // Détecte les impacts sur le joueur et les murs via le trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Vérifie si l'objet touché est bien le joueur via son tag
        if (other.CompareTag("Player"))
        {
            // Inflige un dégât via le singleton LivesManager
            LivesManager.Instance?.TakeDamage();

            AudioManager.Instance?.PlaySFX(_impactClip);

            // Détruit la balle immédiatement après le contact avec le joueur
            Destroy(gameObject);
            return;
        }

        // Vérifie si l'objet touché appartient au layer Wall
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            // Supprime la balle à l'impact avec un mur de la scène
            Destroy(gameObject);
        }
    }
}
