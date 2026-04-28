using UnityEngine;

// Patrouille, détecte le joueur, fonce dessus et se repose
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyCharger : EnemyBase, IEnemyInjectable
{
    // Énumère les quatre états possibles de l'ennemi chargeur
    private enum State { Patrol, Chase, Charge, Cooldown }

    // Vitesse de déplacement lors de la patrouille et de la traque
    [SerializeField] private float _patrolSpeed = 1.5f;

    // Vitesse maximale atteinte lors d'une charge vers le joueur
    [SerializeField] private float _chargeSpeed = 6f;

    // Distance à partir de laquelle le joueur est détecté
    [SerializeField] private float _detectionRadius = 4f;

    // Durée d'attente après une charge avant de reprendre la patrouille
    [SerializeField] private float _chargeCooldown = 1.5f;

    // Durée maximale d'une charge avant retour forcé en cooldown (anti-blocage)
    [SerializeField] private float _chargeMaxDuration = 1.5f;

    // Distance minimale pour déclencher la charge vers le joueur
    [SerializeField] private float _chargeRange = 1.5f;

    // Liste ordonnée des points de passage pour la patrouille
    [SerializeField] private Transform[] _patrolPoints;

    // Référence au gestionnaire de vies du joueur
    [SerializeField] private LivesManager _livesManager;

    // Référence au Transform du joueur pour le suivi de position
    [SerializeField] private Transform _playerTransform;

    // Son joué au déclenchement d'une charge
    [SerializeField] private AudioClip _chargeClip;

    // Son joué quand le charger percute le joueur
    [SerializeField] private AudioClip _impactClip;

    // État courant de la machine à états de l'ennemi
    private State _currentState = State.Patrol;

    // Index du point de patrouille actuellement ciblé
    private int _currentPatrolIndex = 0;

    // Direction verrouillée au déclenchement de la charge
    private Vector2 _chargeDirection;

    // Timer décroissant pendant la phase de cooldown post-charge
    private float _cooldownTimer = 0f;

    // Timer décroissant pendant la charge — force la sortie si aucune collision
    private float _chargeTimer = 0f;

    // Rigidbody2D utilisé pour le déplacement physique lors de la charge
    private Rigidbody2D _rigidbody;

    // Durée en secondes d'immunité après le spawn — bloque la détection et les charges
    [SerializeField] private float _spawnImmunityDuration = 1.25f;

    // Timer décroissant de l'immunité de spawn
    private float _spawnImmunityTimer = 0f;

    /// <summary>Injecte playerTransform et livesManager après un Instantiate runtime.</summary>
    public void InjectDependencies(UnityEngine.Transform playerTransform, LivesManager livesManager, LootSystem lootSystem)
    {
        _playerTransform = playerTransform;
        _livesManager    = livesManager;
    }

    // Initialise la référence de base et vérifie les dépendances
    protected override void Awake()
    {
        // Appelle l'initialisation de la santé définie dans EnemyBase
        base.Awake();

        // Récupère le feedback visuel sur le même GameObject
        _feedback = GetComponent<EnemyFeedback>();

        // Récupère le Rigidbody2D pour le déplacement physique pendant la charge
        _rigidbody = GetComponent<Rigidbody2D>();

        // Initialise le timer d'immunité de spawn
        _spawnImmunityTimer = _spawnImmunityDuration;
    }

    // Fallback : cherche le player si l'injection n'a pas eu lieu avant Start
    private void Start()
    {
        if (_playerTransform == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                _playerTransform = playerGO.transform;
            else
                Debug.LogWarning("[EnemyCharger] Player introuvable après injection — l'ennemi sera inerte.", this);
        }
    }

    // Exécute la logique d'état à chaque frame
    private void Update()
    {
        // Ignore si la référence joueur est absente
        if (_playerTransform == null)
        {
            return;
        }

        // Stoppe toute logique si l'ennemi est mort
        if (IsDead())
        {
            return;
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
            case State.Patrol:
                _feedback?.SetMovementRock(false);
                HandlePatrol();
                break;

            case State.Chase:
                _feedback?.SetMovementRock(true);
                HandleChase();
                break;

            case State.Charge:
                _feedback?.SetMovementRock(true);
                HandleCharge();
                break;

            case State.Cooldown:
                _feedback?.SetMovementRock(false);
                HandleCooldown();
                break;
        }
    }

    // Déplace l'ennemi entre les points de patrouille en boucle
    private void HandlePatrol()
    {
        // Interrompt la patrouille si aucun point n'est configuré
        if (_patrolPoints == null || _patrolPoints.Length == 0)
        {
            return;
        }

        // Récupère la position du point de patrouille actuel
        Vector3 target = _patrolPoints[_currentPatrolIndex].position;

        // Déplace l'ennemi vers le point cible à vitesse de patrouille
        transform.position = Vector3.MoveTowards(transform.position, target, _patrolSpeed * Time.deltaTime);

        // Passe au point suivant si l'ennemi est suffisamment proche
        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            // Calcule l'index du prochain point en bouclant sur le tableau
            _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;
        }

        // Passe en mode traque si le joueur entre dans le rayon de détection
        if (IsPlayerInDetectionRadius())
        {
            // Transition vers l'état de traque du joueur
            _currentState = State.Chase;
        }
    }

    // Déclenche la charge vers le joueur si assez proche
    private void HandleChase()
    {
        // Repasse en patrouille si le joueur sort du rayon de détection
        if (!IsPlayerInDetectionRadius())
        {
            // Reprend la patrouille si le joueur est trop loin
            _currentState = State.Patrol;
            return;
        }

        // Déplace l'ennemi vers le joueur à vitesse de patrouille
        transform.position = Vector3.MoveTowards(
            transform.position,
            _playerTransform.position,
            _patrolSpeed * Time.deltaTime
        );

        // Déclenche la charge si l'ennemi est assez proche du joueur
        if (Vector3.Distance(transform.position, _playerTransform.position) <= _chargeRange)
        {
            // Verrouille la direction vers le joueur pour la charge
            _chargeDirection = (_playerTransform.position - transform.position).normalized;

            // Arme le timer de durée maximale de charge (anti-blocage)
            _chargeTimer = _chargeMaxDuration;

            AudioManager.Instance?.PlaySFX(_chargeClip);

            // Passe immédiatement dans l'état de charge rapide
            _currentState = State.Charge;
        }
    }

    // Gère le timer de charge et la transition vers le cooldown si la durée est dépassée.
    // Le déplacement physique est appliqué dans FixedUpdate.
    private void HandleCharge()
    {
        _chargeTimer -= Time.deltaTime;

        // Force la sortie de charge si la durée maximale est dépassée (anti-blocage mur)
        if (_chargeTimer <= 0f)
        {
            StopCharge();
        }
    }

    // Applique le déplacement via Rigidbody2D.MovePosition pour déclencher les collisions physiques.
    private void FixedUpdate()
    {
        if (IsDead() || _currentState != State.Charge) return;

        Vector2 nextPos = _rigidbody.position + _chargeDirection * (_chargeSpeed * Time.fixedDeltaTime);
        _rigidbody.MovePosition(nextPos);
    }

    // Arrête la charge et passe en cooldown.
    private void StopCharge()
    {
        _rigidbody.linearVelocity = Vector2.zero;
        _cooldownTimer  = _chargeCooldown;
        _currentState   = State.Cooldown;
    }

    // Décrémente le timer et reprend la patrouille à son expiration
    private void HandleCooldown()
    {
        // Réduit le timer du cooldown avec le temps écoulé cette frame
        _cooldownTimer -= Time.deltaTime;

        // Reprend la patrouille une fois le cooldown entièrement écoulé
        if (_cooldownTimer <= 0f)
        {
            // Transition vers l'état de patrouille
            _currentState = State.Patrol;
        }
    }

    // Retourne vrai si le joueur est dans le rayon de détection
    private bool IsPlayerInDetectionRadius()
    {
        // Vérifie que la référence au joueur est bien assignée
        if (_playerTransform == null)
        {
            return false;
        }

        // Compare la distance au joueur avec le rayon de détection
        return Vector3.Distance(transform.position, _playerTransform.position) <= _detectionRadius;
    }

    // Détecte la collision avec le joueur pendant la phase de charge
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignore les collisions si l'ennemi n'est pas en état de charge
        if (_currentState != State.Charge)
            return;

        // Inflige des dégâts au joueur si le collider est bien le sien
        if (collision.gameObject.CompareTag("Player"))
        {
            if (_livesManager != null)
                _livesManager.TakeDamage();

            AudioManager.Instance?.PlaySFX(_impactClip);
        }

        // Stoppe la charge dans tous les cas (mur ou joueur) et passe en cooldown
        StopCharge();
    }

    // Désactive le GameObject à la mort de l'ennemi chargeur
    protected override void HandleDeath()
    {
        // Désactive l'objet entier pour le retirer de la scène
        gameObject.SetActive(false);
    }
}
