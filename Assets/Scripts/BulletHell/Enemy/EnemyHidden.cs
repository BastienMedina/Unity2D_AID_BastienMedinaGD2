using UnityEngine;

// Spawne depuis un bureau fouillé infesté, attaque le joueur, puis se cache dans un autre bureau.
public class EnemyHidden : EnemyBase, IEnemyInjectable
{
    // -------------------------------------------------------------------------
    // États de la machine à états
    // -------------------------------------------------------------------------

    private enum State { Idle, Attacking, MovingToDesk, Hidden }

    // -------------------------------------------------------------------------
    // Paramètres configurables
    // -------------------------------------------------------------------------

    // Vitesse de déplacement vers le joueur ou vers un bureau
    [SerializeField] private float _moveSpeed = 3f;

    // Durée de la phase d'attaque avant de chercher un bureau
    [SerializeField] private float _attackDuration = 4f;

    // Délai minimum entre deux infligements de dégâts au joueur
    [SerializeField] private float _damageCooldown = 1f;

    // Distance de contact pour infliger des dégâts au joueur
    [SerializeField] private float _contactRadius = 0.4f;

    // Son joué quand l'ennemi émerge d'un bureau
    [SerializeField] private AudioClip _revealClip;

    // Son joué pendant la phase d'attaque de l'ennemi
    [SerializeField] private AudioClip _attackClip;

    // -------------------------------------------------------------------------
    // Références résolues à l'exécution
    // -------------------------------------------------------------------------

    private Transform _playerTransform;
    private LivesManager _livesManager;
    private LootSystem _lootSystem;

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    private State _currentState = State.Idle;
    private SearchableObject _targetDesk;
    private SearchableObject _currentDesk;
    private float _attackTimer;
    private float _damageTimer;

    // -------------------------------------------------------------------------
    // IEnemyInjectable — no-op : EnemyHidden résout ses propres dépendances
    // -------------------------------------------------------------------------

    /// <summary>No-op : EnemyHidden est spawné par les bureaux, pas par SpawnEnemies.</summary>
    public void InjectDependencies(Transform playerTransform, LivesManager livesManager, LootSystem lootSystem) { }

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    protected override void Awake()
    {
        base.Awake();

        // Résolution automatique des dépendances depuis la scène
        _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        _livesManager    = FindFirstObjectByType<LivesManager>();
        _lootSystem      = FindFirstObjectByType<LootSystem>();

        // S'assure que HiddenVisual est désactivé au spawn
        ShowHiddenVisual(false);

        // Inactif jusqu'à être déclenché par un bureau fouillé
        gameObject.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // API publique — appelée par SearchableObject
    // -------------------------------------------------------------------------

    /// <summary>Spawne l'ennemi à la position donnée et déclenche la phase d'attaque.</summary>
    public void SpawnAndAttack(Vector3 position)
    {
        transform.position = position;
        _currentDesk       = null;
        _attackTimer       = _attackDuration;
        // Initialise le timer à plein cooldown pour éviter un dégât instantané au spawn
        _damageTimer       = _damageCooldown;
        _currentState      = State.Attacking;
        ShowHiddenVisual(false);
        gameObject.SetActive(true);
    }

    /// <summary>Révèle l'ennemi depuis le bureau où il se cachait et relance l'attaque.</summary>
    public void RevealFromDesk()
    {
        _currentDesk  = null;
        _attackTimer  = _attackDuration;
        // Initialise le timer à plein cooldown pour éviter un dégât instantané à la révélation
        _damageTimer  = _damageCooldown;
        _currentState = State.Attacking;
        ShowHiddenVisual(false);
        AudioManager.Instance?.PlaySFX(_revealClip);
        gameObject.SetActive(true);
    }

    // -------------------------------------------------------------------------
    // Boucle principale
    // -------------------------------------------------------------------------

    private void Update()
    {
        if (IsDead()) return;

        switch (_currentState)
        {
            case State.Attacking:    HandleAttacking();    break;
            case State.MovingToDesk: HandleMovingToDesk(); break;
        }
    }

    // -------------------------------------------------------------------------
    // Phase d'attaque
    // -------------------------------------------------------------------------

    // Fonce vers le joueur, inflige des dégâts au contact, puis cherche un bureau
    private void HandleAttacking()
    {
        _attackTimer -= Time.deltaTime;
        _damageTimer -= Time.deltaTime;

        if (_playerTransform != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                _playerTransform.position,
                _moveSpeed * Time.deltaTime);

            if (_damageTimer <= 0f &&
                Vector2.Distance(transform.position, _playerTransform.position) <= _contactRadius)
            {
                _livesManager?.TakeDamage();
                AudioManager.Instance?.PlaySFX(_attackClip);
                _damageTimer = _damageCooldown;
            }
        }

        if (_attackTimer <= 0f)
            StartMoveToDesk();
    }

    // -------------------------------------------------------------------------
    // Recherche d'un bureau disponible
    // -------------------------------------------------------------------------

    // Trouve le bureau non fouillé le plus proche et initialise le déplacement
    private void StartMoveToDesk()
    {
        SearchableObject[] all = FindObjectsByType<SearchableObject>(FindObjectsSortMode.None);
        SearchableObject best  = null;
        float bestDist         = float.MaxValue;

        foreach (SearchableObject desk in all)
        {
            // Ignore les bureaux déjà fouillés, le bureau précédent et ceux déjà occupés
            if (desk.GetState() == SearchableObject.SearchState.Searched) continue;
            if (desk == _currentDesk) continue;
            if (!desk.CanHideEnemy()) continue;

            float d = Vector2.Distance(transform.position, desk.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = desk;
            }
        }

        if (best == null)
        {
            // Aucun bureau disponible : l'ennemi disparaît
            gameObject.SetActive(false);
            return;
        }

        _targetDesk   = best;
        _currentState = State.MovingToDesk;
    }

    // -------------------------------------------------------------------------
    // Phase de déplacement vers un bureau
    // -------------------------------------------------------------------------

    // Se déplace vers le bureau cible et s'y cache à l'arrivée
    private void HandleMovingToDesk()
    {
        if (_targetDesk == null || _targetDesk.GetState() == SearchableObject.SearchState.Searched)
        {
            StartMoveToDesk();
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            _targetDesk.transform.position,
            _moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, _targetDesk.transform.position) < 0.2f)
        {
            _targetDesk.RegisterHiddenEnemy(this);
            _currentDesk  = _targetDesk;
            _targetDesk   = null;
            _currentState = State.Hidden;
            ShowHiddenVisual(true);
            gameObject.SetActive(false);
        }
    }

    // -------------------------------------------------------------------------
    // Mort
    // -------------------------------------------------------------------------

    protected override void HandleDeath()
    {
        CancelInvoke();
        _currentDesk?.UnregisterHiddenEnemy();
        _lootSystem?.SpawnLoot(transform.position);
        gameObject.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Visuel caché / révélé
    // -------------------------------------------------------------------------

    // Cache le visuel "bureau" sur l'enfant HiddenVisual (activé quand caché dans un bureau)
    private void ShowHiddenVisual(bool show)
    {
        Transform t = transform.Find("HiddenVisual");
        if (t != null)
            t.gameObject.SetActive(show);
    }
}
