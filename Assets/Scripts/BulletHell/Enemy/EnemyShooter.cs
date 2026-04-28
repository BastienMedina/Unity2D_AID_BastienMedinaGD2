using UnityEngine;

public class EnemyShooter : EnemyBase, IEnemyInjectable
{
    private enum State { Idle, Aim, Shoot, Retreat }

    [SerializeField] private float _detectionRadius = 6f;
    [SerializeField] private float _preferredDistance = 4f;
    [SerializeField] private float _retreatSpeed = 2f;
    [SerializeField] private float _shootCooldown = 2f;
    [SerializeField] private float _detectionCheckInterval = 0.3f;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private int _projectileDamage = 1;
    [SerializeField] private float _projectileSpeed = 6f;
    [SerializeField] private float _projectileSpawnOffset = 0.5f;
    [SerializeField] private float _projectileMaxRange = 10f;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private LivesManager _livesManager;
    [SerializeField] private LootSystem _lootSystem;
    [SerializeField] private float _spawnImmunityDuration = 1.25f;
    [SerializeField] private AudioClip _shootClip;

    private float _spawnImmunityTimer;
    private State _currentState = State.Idle;
    private float _shootCooldownTimer;

    public void InjectDependencies(Transform playerTransform, LivesManager livesManager, LootSystem lootSystem) // Injecte les dépendances après Instantiate
    {
        _playerTransform = playerTransform;
        _livesManager    = livesManager;
        _lootSystem      = lootSystem;
    }

    protected override void Awake() // Initialise et lance la vérification périodique
    {
        base.Awake();
        _spawnImmunityTimer = _spawnImmunityDuration;
        InvokeRepeating(nameof(CheckDetection), 0f, _detectionCheckInterval);
    }

    private void Start() // Cherche le joueur si injection manquante
    {
        if (_playerTransform == null) // Fallback par tag si non injecté
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                _playerTransform = playerGO.transform;
            else
                Debug.LogWarning("[EnemyShooter] Player introuvable — ennemi inerte.", this);
        }
    }

    private void Update() // Gère le cooldown de tir et exécute l'état courant
    {
        if (IsDead())
            return;

        if (_shootCooldownTimer > 0f)
            _shootCooldownTimer -= Time.deltaTime;

        if (_spawnImmunityTimer > 0f) // Bloque toute action pendant l'immunité
        {
            _spawnImmunityTimer -= Time.deltaTime;
            return;
        }

        switch (_currentState) // Redirige vers le handler d'état
        {
            case State.Idle:    break;
            case State.Aim:     HandleAim();     break;
            case State.Shoot:   HandleShoot();   break;
            case State.Retreat: HandleRetreat(); break;
        }
    }

    private void CheckDetection() // Met à jour l'état selon la distance au joueur
    {
        if (IsDead() || _playerTransform == null)
            return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);

        if (dist > _detectionRadius) { _currentState = State.Idle;    return; } // Hors portée
        if (dist < _preferredDistance) { _currentState = State.Retreat; return; } // Trop proche
        if (_shootCooldownTimer <= 0f) { _currentState = State.Shoot;   return; } // Prêt à tirer

        _currentState = State.Aim; // Attend le cooldown en visant
    }

    private void HandleAim() // Oriente le sprite vers le joueur
    {
        if (_playerTransform == null) return;
        Vector2 dir   = _playerTransform.position - transform.position;
        float angle   = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void HandleShoot() // Instancie un projectile orienté vers le joueur
    {
        if (_playerTransform == null) return;

        HandleAim();
        Vector2 direction    = (_playerTransform.position - transform.position).normalized;
        Vector3 spawnPos     = transform.position + (Vector3)(direction * _projectileSpawnOffset);
        GameObject projObj   = Instantiate(_projectilePrefab, spawnPos, Quaternion.identity);
        EnemyProjectile proj = projObj.GetComponent<EnemyProjectile>();
        proj.Initialize(direction, _projectileSpeed, _projectileMaxRange, _livesManager, _projectileDamage);
        AudioManager.Instance?.PlaySFX(_shootClip);
        _feedback?.PlayShootFeedback();
        _shootCooldownTimer = _shootCooldown;
        _currentState       = State.Aim;
    }

    private void HandleRetreat() // Recule pour maintenir la distance préférée
    {
        if (_playerTransform == null) return;

        Vector2 retreatDir = (transform.position - _playerTransform.position).normalized;
        Vector2 aimDir     = _playerTransform.position - transform.position;
        float angle        = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.Translate(retreatDir * (_retreatSpeed * Time.deltaTime), Space.World);
    }

    protected override void HandleDeath() // Désactive et spawne le loot
    {
        CancelInvoke();
        gameObject.SetActive(false);
        _lootSystem?.SpawnLoot(transform.position);
    }
}
