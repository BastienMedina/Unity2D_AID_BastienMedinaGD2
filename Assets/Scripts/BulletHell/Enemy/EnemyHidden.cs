using UnityEngine;

public class EnemyHidden : EnemyBase, IEnemyInjectable
{
    private enum State { Idle, Attacking, MovingToDesk, Hidden }

    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _attackDuration = 4f;
    [SerializeField] private float _damageCooldown = 1f;
    [SerializeField] private float _contactRadius = 0.4f;
    [SerializeField] private AudioClip _revealClip;
    [SerializeField] private AudioClip _attackClip;

    private Transform _playerTransform;
    private LivesManager _livesManager;
    private LootSystem _lootSystem;

    private State _currentState = State.Idle;
    private SearchableObject _targetDesk;
    private SearchableObject _currentDesk;
    private float _attackTimer;
    private float _damageTimer;

    public void InjectDependencies(Transform playerTransform, LivesManager livesManager, LootSystem lootSystem) { } // No-op : résout ses propres dépendances

    protected override void Awake() // Résout les dépendances depuis la scène et se cache
    {
        base.Awake();
        _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        _livesManager    = FindFirstObjectByType<LivesManager>();
        _lootSystem      = FindFirstObjectByType<LootSystem>();
        ShowHiddenVisual(false);
        gameObject.SetActive(false);
    }

    public void SpawnAndAttack(Vector3 position) // Apparaît à la position donnée et attaque
    {
        transform.position = position;
        _currentDesk       = null;
        _attackTimer       = _attackDuration;
        _damageTimer       = _damageCooldown; // Évite un dégât instantané au spawn
        _currentState      = State.Attacking;
        ShowHiddenVisual(false);
        gameObject.SetActive(true);
    }

    public void RevealFromDesk() // Émerge du bureau caché et relance l'attaque
    {
        _currentDesk  = null;
        _attackTimer  = _attackDuration;
        _damageTimer  = _damageCooldown; // Évite un dégât instantané à la révélation
        _currentState = State.Attacking;
        ShowHiddenVisual(false);
        AudioManager.Instance?.PlaySFX(_revealClip);
        gameObject.SetActive(true);
    }

    private void Update() // Exécute le comportement selon l'état courant
    {
        if (IsDead()) return;

        switch (_currentState) // Redirige vers le handler d'état actif
        {
            case State.Attacking:    HandleAttacking();    break;
            case State.MovingToDesk: HandleMovingToDesk(); break;
        }
    }

    private void HandleAttacking() // Fonce vers le joueur et inflige dégâts au contact
    {
        _attackTimer -= Time.deltaTime;
        _damageTimer -= Time.deltaTime;

        if (_playerTransform != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, _playerTransform.position, _moveSpeed * Time.deltaTime);

            if (_damageTimer <= 0f && Vector2.Distance(transform.position, _playerTransform.position) <= _contactRadius) // Inflige dégâts si contact
            {
                _livesManager?.TakeDamage();
                AudioManager.Instance?.PlaySFX(_attackClip);
                _damageTimer = _damageCooldown;
            }
        }

        if (_attackTimer <= 0f) // Cherche un bureau après la phase d'attaque
            StartMoveToDesk();
    }

    private void StartMoveToDesk() // Trouve le bureau non fouillé le plus proche
    {
        SearchableObject[] all = FindObjectsByType<SearchableObject>(FindObjectsSortMode.None);
        SearchableObject best  = null;
        float bestDist         = float.MaxValue;

        foreach (SearchableObject desk in all) // Parcourt tous les bureaux disponibles
        {
            if (desk.GetState() == SearchableObject.SearchState.Searched) continue; // Ignore les bureaux fouillés
            if (desk == _currentDesk) continue;
            if (!desk.CanHideEnemy()) continue;

            float d = Vector2.Distance(transform.position, desk.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best     = desk;
            }
        }

        if (best == null) // Aucun bureau disponible : disparaît
        {
            gameObject.SetActive(false);
            return;
        }

        _targetDesk   = best;
        _currentState = State.MovingToDesk;
    }

    private void HandleMovingToDesk() // Se déplace vers le bureau cible et s'y cache
    {
        if (_targetDesk == null || _targetDesk.GetState() == SearchableObject.SearchState.Searched)
        {
            StartMoveToDesk(); // Cherche un autre bureau si la cible est invalide
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, _targetDesk.transform.position, _moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, _targetDesk.transform.position) < 0.2f) // S'enregistre dans le bureau à l'arrivée
        {
            _targetDesk.RegisterHiddenEnemy(this);
            _currentDesk  = _targetDesk;
            _targetDesk   = null;
            _currentState = State.Hidden;
            ShowHiddenVisual(true);
            gameObject.SetActive(false);
        }
    }

    protected override void HandleDeath() // Libère le bureau et spawne le loot
    {
        CancelInvoke();
        _currentDesk?.UnregisterHiddenEnemy();
        _lootSystem?.SpawnLoot(transform.position);
        gameObject.SetActive(false);
    }

    private void ShowHiddenVisual(bool show) // Active ou désactive le visuel bureau
    {
        Transform t = transform.Find("HiddenVisual");
        if (t != null) t.gameObject.SetActive(show);
    }
}
