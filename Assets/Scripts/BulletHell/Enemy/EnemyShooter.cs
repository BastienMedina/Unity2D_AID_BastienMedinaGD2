using UnityEngine;

// Détecte le joueur, maintient une distance et tire des projectiles
public class EnemyShooter : EnemyBase, IEnemyInjectable
{
    // Énumère les quatre états du tireur à distance
    private enum State { Idle, Aim, Shoot, Retreat }

    // Distance maximale à laquelle le joueur est détecté
    [SerializeField] private float _detectionRadius = 6f;

    // Distance idéale maintenue entre le tireur et le joueur
    [SerializeField] private float _preferredDistance = 4f;

    // Vitesse de retraite quand le joueur est trop proche
    [SerializeField] private float _retreatSpeed = 2f;

    // Délai minimal entre deux tirs consécutifs en secondes
    [SerializeField] private float _shootCooldown = 2f;

    // Intervalle de vérification de proximité joueur en secondes
    [SerializeField] private float _detectionCheckInterval = 0.3f;

    // Préfab du projectile instancié à chaque tir
    [SerializeField] private GameObject _projectilePrefab;

    // Dégâts transmis au projectile lors de son instanciation
    [SerializeField] private int _projectileDamage = 1;

    // Vitesse de déplacement du projectile après instanciation
    [SerializeField] private float _projectileSpeed = 6f;

    // Portée maximale du projectile avant auto-destruction
    [SerializeField] private float _projectileMaxRange = 10f;

    // Référence au Transform du joueur pour le calcul de direction
    [SerializeField] private Transform _playerTransform;

    // Gestionnaire de vies du joueur touché par un projectile
    [SerializeField] private LivesManager _livesManager;

    // Gestionnaire de butin déclenché à la mort du tireur
    [SerializeField] private LootSystem _lootSystem;

    // Durée en secondes d'immunité après le spawn — bloque toute action offensive
    [SerializeField] private float _spawnImmunityDuration = 1.25f;

    // Son joué lors d'un tir
    [SerializeField] private AudioClip _shootClip;

    // Timer décroissant de l'immunité de spawn
    private float _spawnImmunityTimer = 0f;

    // État courant dans le cycle de la machine à états
    private State _currentState = State.Idle;

    // Timer décroissant entre deux tirs successifs
    private float _shootCooldownTimer = 0f;

    /// <summary>Injecte playerTransform, livesManager et lootSystem après un Instantiate runtime.</summary>
    public void InjectDependencies(UnityEngine.Transform playerTransform, LivesManager livesManager, LootSystem lootSystem)
    {
        _playerTransform = playerTransform;
        _livesManager    = livesManager;
        _lootSystem      = lootSystem;
    }

    // Initialise la santé et démarre la vérification périodique
    protected override void Awake()
    {
        // Appelle l'initialisation de la santé définie dans EnemyBase
        base.Awake();

        // Récupère le feedback visuel sur le même GameObject
        _feedback = GetComponent<EnemyFeedback>();

        // Initialise le timer d'immunité de spawn
        _spawnImmunityTimer = _spawnImmunityDuration;

        // Lance la vérification de proximité à intervalle régulier
        InvokeRepeating(nameof(CheckDetection), 0f, _detectionCheckInterval);
    }

    // Gère la logique de tir et de retraite à chaque frame
    private void Update()
    {
        // Stoppe toute logique si l'ennemi est mort
        if (IsDead())
        {
            return;
        }

        // Décrémente le cooldown de tir si encore actif
        if (_shootCooldownTimer > 0f)
        {
            _shootCooldownTimer -= Time.deltaTime;
        }

        // Décrémente l'immunité de spawn et bloque toute action tant qu'elle est active
        if (_spawnImmunityTimer > 0f)
        {
            _spawnImmunityTimer -= Time.deltaTime;
            return;
        }

        // Redirige vers la méthode correspondant à l'état courant
        switch (_currentState)
        {
            // Aucune action en état d'attente, la détection est périodique
            case State.Idle:
                break;

            // Oriente le tireur vers le joueur sans se déplacer
            case State.Aim:
                HandleAim();
                break;

            // Tire un projectile si le cooldown est expiré
            case State.Shoot:
                HandleShoot();
                break;

            // Recule pour maintenir la distance préférée avec le joueur
            case State.Retreat:
                HandleRetreat();
                break;
        }
    }

    // Vérifie la distance joueur et met à jour l'état en conséquence
    private void CheckDetection()
    {
        // Stoppe la détection si l'ennemi est mort
        if (IsDead())
        {
            return;
        }

        // Ignore si la référence au joueur n'est pas assignée
        if (_playerTransform == null)
        {
            return;
        }

        // Calcule la distance actuelle entre le tireur et le joueur
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

        // Repasse en attente si le joueur est hors du rayon de détection
        if (distanceToPlayer > _detectionRadius)
        {
            _currentState = State.Idle;
            return;
        }

        // Passe en retraite si le joueur est trop proche
        if (distanceToPlayer < _preferredDistance)
        {
            _currentState = State.Retreat;
            return;
        }

        // Passe en tir si le cooldown est expiré et le joueur en vue
        if (_shootCooldownTimer <= 0f)
        {
            _currentState = State.Shoot;
            return;
        }

        // Oriente le tireur vers le joueur en attendant le prochain tir
        _currentState = State.Aim;
    }

    // Oriente le sprite du tireur vers la position du joueur
    private void HandleAim()
    {
        // Ignore si la référence au joueur est manquante
        if (_playerTransform == null)
        {
            return;
        }

        // Calcule le vecteur direction non normalisé vers le joueur
        Vector2 directionToPlayer = _playerTransform.position - transform.position;

        // Soustrait 90° pour que le haut du sprite (axe Y local) pointe vers le joueur
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg - 90f;

        // Applique la rotation calculée à l'axe Z du Transform
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    // Instancie un projectile orienté vers le joueur si possible
    private void HandleShoot()
    {
        // Ignore le tir si le joueur n'est pas référencé
        if (_playerTransform == null)
        {
            return;
        }

        // Oriente le tireur avant d'instancier le projectile
        HandleAim();

        // Calcule la direction normalisée vers le joueur pour le tir
        Vector2 direction = (_playerTransform.position - transform.position).normalized;

        // Instancie le projectile à la position actuelle du tireur
        GameObject projectileObject = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);

        // Récupère le script EnemyProjectile sur l'objet instancié
        EnemyProjectile projectile = projectileObject.GetComponent<EnemyProjectile>();

        // Initialise le projectile avec direction, vitesse, portée et dégâts
        projectile.Initialize(direction, _projectileSpeed, _projectileMaxRange, _livesManager, _projectileDamage);

        AudioManager.Instance?.PlaySFX(_shootClip);

        // Joue le feedback visuel de tir
        _feedback?.PlayShootFeedback();

        // Réarme le cooldown de tir après un tir réussi
        _shootCooldownTimer = _shootCooldown;

        // Passe en état de visée en attendant le prochain tir
        _currentState = State.Aim;
    }

    // Déplace le tireur à l'opposé du joueur pour maintenir la distance
    private void HandleRetreat()
    {
        // Ignore la retraite si le joueur n'est pas référencé
        if (_playerTransform == null)
        {
            return;
        }

        // Calcule la direction opposée au joueur pour reculer
        Vector2 retreatDirection = (transform.position - _playerTransform.position).normalized;

        // Continue d'orienter le tireur vers le joueur pendant la retraite
        Vector2 directionToPlayer = _playerTransform.position - transform.position;
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Déplace le tireur en sens opposé du joueur cette frame
        transform.Translate(retreatDirection * (_retreatSpeed * Time.deltaTime), Space.World);
    }

    // Désactive l'ennemi et déclenche le butin à la mort
    protected override void HandleDeath()
    {
        // Annule les vérifications périodiques de proximité joueur
        CancelInvoke();

        // Désactive le GameObject pour le retirer visuellement de la scène
        gameObject.SetActive(false);

        // Demande au système de butin de spawner le loot à cet endroit
        _lootSystem?.SpawnLoot(transform.position);
    }
}
