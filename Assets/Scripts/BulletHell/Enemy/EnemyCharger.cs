using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyCharger : EnemyBase, IEnemyInjectable
{
    private enum State { Patrol, Chase, Charge, Cooldown }

    [SerializeField] private float _patrolSpeed = 1.5f;
    [SerializeField] private float _chargeSpeed = 6f;
    [SerializeField] private float _detectionRadius = 4f;
    [SerializeField] private float _chargeCooldown = 1.5f;
    [SerializeField] private float _chargeMaxDuration = 1.5f;
    [SerializeField] private float _chargeRange = 1.5f;
    [SerializeField] private Transform[] _patrolPoints;
    [SerializeField] private LivesManager _livesManager;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private AudioClip _chargeClip;
    [SerializeField] private AudioClip _impactClip;
    [SerializeField] private float _spawnImmunityDuration = 1.25f;

    private State _currentState = State.Patrol;
    private int _currentPatrolIndex;
    private Vector2 _chargeDirection;
    private float _cooldownTimer;
    private float _chargeTimer;
    private float _spawnImmunityTimer;
    private Rigidbody2D _rigidbody;

    public void InjectDependencies(Transform playerTransform, LivesManager livesManager, LootSystem lootSystem) // Injecte les dépendances après Instantiate
    {
        _playerTransform = playerTransform;
        _livesManager    = livesManager;
    }

    protected override void Awake() // Initialise composants et immunité de spawn
    {
        base.Awake();
        _rigidbody          = GetComponent<Rigidbody2D>();
        _spawnImmunityTimer = _spawnImmunityDuration;
    }

    private void Start() // Cherche le joueur si injection manquante
    {
        if (_playerTransform == null) // Fallback par tag si non injecté
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                _playerTransform = playerGO.transform;
            else
                Debug.LogWarning("[EnemyCharger] Player introuvable — ennemi inerte.", this);
        }
    }

    private void Update() // Exécute le comportement selon l'état courant
    {
        if (_playerTransform == null || IsDead())
            return;

        if (_spawnImmunityTimer > 0f) // Bloque toute action pendant l'immunité
        {
            _spawnImmunityTimer -= Time.deltaTime;
            return;
        }

        switch (_currentState) // Redirige vers le handler d'état
        {
            case State.Patrol:   _feedback?.SetMovementRock(false); HandlePatrol();   break;
            case State.Chase:    _feedback?.SetMovementRock(true);  HandleChase();    break;
            case State.Charge:   _feedback?.SetMovementRock(true);  HandleCharge();   break;
            case State.Cooldown: _feedback?.SetMovementRock(false); HandleCooldown(); break;
        }
    }

    private void HandlePatrol() // Déplace entre les points de patrouille en boucle
    {
        if (_patrolPoints == null || _patrolPoints.Length == 0)
            return;

        Vector3 target = _patrolPoints[_currentPatrolIndex].position;
        transform.position = Vector3.MoveTowards(transform.position, target, _patrolSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.1f) // Change de cible si arrivé
            _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;

        if (IsPlayerInDetectionRadius()) // Passe en traque si joueur détecté
            _currentState = State.Chase;
    }

    private void HandleChase() // Poursuit le joueur et déclenche la charge si proche
    {
        if (!IsPlayerInDetectionRadius()) // Repasse en patrouille si joueur perdu
        {
            _currentState = State.Patrol;
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, _playerTransform.position, _patrolSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _playerTransform.position) <= _chargeRange) // Lance la charge si assez proche
        {
            _chargeDirection = (_playerTransform.position - transform.position).normalized;
            _chargeTimer     = _chargeMaxDuration;
            AudioManager.Instance?.PlaySFX(_chargeClip);
            _currentState = State.Charge;
        }
    }

    private void HandleCharge() // Gère le timeout anti-blocage de la charge
    {
        _chargeTimer -= Time.deltaTime;
        if (_chargeTimer <= 0f) // Stoppe la charge si durée dépassée
            StopCharge();
    }

    private void FixedUpdate() // Applique le déplacement physique en charge
    {
        if (IsDead() || _currentState != State.Charge) return;
        _rigidbody.MovePosition(_rigidbody.position + _chargeDirection * (_chargeSpeed * Time.fixedDeltaTime));
    }

    private void StopCharge() // Passe en cooldown après la charge
    {
        _rigidbody.linearVelocity = Vector2.zero;
        _cooldownTimer            = _chargeCooldown;
        _currentState             = State.Cooldown;
    }

    private void HandleCooldown() // Attend avant de reprendre la patrouille
    {
        _cooldownTimer -= Time.deltaTime;
        if (_cooldownTimer <= 0f) // Repasse en patrouille si délai écoulé
            _currentState = State.Patrol;
    }

    private bool IsPlayerInDetectionRadius() // Vérifie si le joueur est à portée
    {
        return _playerTransform != null &&
               Vector3.Distance(transform.position, _playerTransform.position) <= _detectionRadius;
    }

    private void OnCollisionEnter2D(Collision2D collision) // Inflige dégâts si impact joueur en charge
    {
        if (_currentState != State.Charge)
            return;

        if (collision.gameObject.CompareTag("Player")) // Dégâts uniquement sur le joueur
        {
            _livesManager?.TakeDamage();
            AudioManager.Instance?.PlaySFX(_impactClip);
        }

        StopCharge();
    }

    protected override void HandleDeath() // Désactive l'objet à la mort
    {
        gameObject.SetActive(false);
    }
}
