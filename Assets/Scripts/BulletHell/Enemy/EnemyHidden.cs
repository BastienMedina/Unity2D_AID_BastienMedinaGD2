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

        // Applique le visuel orange caractéristique de cet ennemi
        ApplyVisual();

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
        _damageTimer       = 0f;
        _currentState      = State.Attacking;
        gameObject.SetActive(true);
    }

    /// <summary>Révèle l'ennemi depuis le bureau où il se cachait et relance l'attaque.</summary>
    public void RevealFromDesk()
    {
        _currentDesk  = null;
        _attackTimer  = _attackDuration;
        _damageTimer  = 0f;
        _currentState = State.Attacking;
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
    // Visuel
    // -------------------------------------------------------------------------

    // Applique le sprite orange directement sur ce GameObject
    private void ApplyVisual()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>()
                         ?? gameObject.AddComponent<SpriteRenderer>();

        Texture2D tex    = new Texture2D(1, 1);
        tex.filterMode   = FilterMode.Point;
        tex.SetPixel(0, 0, new Color(1f, 0.4f, 0f, 1f));
        tex.Apply();

        sr.sprite         = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        sr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
        sr.sortingOrder   = 2;
        transform.localScale = new Vector3(0.6f, 0.6f, 1f);
    }
}
