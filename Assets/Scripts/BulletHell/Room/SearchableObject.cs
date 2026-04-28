using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SearchableObject : MonoBehaviour
{
    public enum SearchState { Idle, PlayerNearby, Searching, Searched }

    [SerializeField] private float _searchMargin = 0.8f;
    [SerializeField] private float _searchDuration = 1.5f;
    [SerializeField] private LootDropper _lootDropper;
    [SerializeField] private DeskNetworkEnemy _networkEnemy;
    [SerializeField] private string _objectLabel = "Bureau";
    [SerializeField] private float _proximityCheckInterval = 0.2f;
    [SerializeField][Range(0f, 1f)] private float _infestedChance = 0f;
    [SerializeField] private GameObject _enemyHiddenPrefab;

    private EnemyHidden _hiddenEnemy;

    public UnityEvent OnSearchStarted                              = new UnityEvent();
    public UnityEvent<SearchableObject> OnPlayerEnterRange         = new UnityEvent<SearchableObject>();
    public UnityEvent<SearchableObject> OnPlayerExitRange          = new UnityEvent<SearchableObject>();
    public UnityEvent OnSearchComplete                             = new UnityEvent();
    public UnityEvent<float> OnSearchProgressUpdate               = new UnityEvent<float>();

    private SearchState _state = SearchState.Idle;
    private Transform _playerTransform;
    private Coroutine _searchCoroutine;

    private void Awake() // Localise le joueur par tag
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            _playerTransform = playerObject.transform;
    }

    private void Start() // Démarre la vérification périodique de proximité
    {
        if (_playerTransform == null) return;
        InvokeRepeating(nameof(CheckPlayerProximity), 0f, _proximityCheckInterval);
    }

    private void OnDestroy() // Annule la vérification périodique
    {
        CancelInvoke(nameof(CheckPlayerProximity));
    }

    public void StartSearch() // Démarre la fouille si joueur proche et non fouillé
    {
        if (_state == SearchState.Searched || _state != SearchState.PlayerNearby) return;
        _state = SearchState.Searching;
        OnSearchStarted.Invoke();
        _searchCoroutine = StartCoroutine(SearchCoroutine());
    }

    public void CancelSearch() // Annule la fouille et revient à PlayerNearby
    {
        if (_state != SearchState.Searching) return;
        if (_searchCoroutine != null) { StopCoroutine(_searchCoroutine); _searchCoroutine = null; }
        _state = SearchState.PlayerNearby;
    }

    public bool IsSearched()       => _state == SearchState.Searched; // Retourne si déjà fouillé
    public SearchState GetState()  => _state;                          // Expose l'état courant
    public string GetLabel()       => _objectLabel;                    // Retourne l'étiquette de l'objet
    public void SetLabel(string label)             => _objectLabel  = label;           // Met à jour l'étiquette
    public void SetLootDropper(LootDropper ld)     => _lootDropper  = ld;              // Assigne le LootDropper
    public void SetInfested(float chance, GameObject prefab) { _infestedChance = chance; _enemyHiddenPrefab = prefab; } // Configure l'infestation
    public void RegisterHiddenEnemy(EnemyHidden e) => _hiddenEnemy  = e;              // Enregistre l'ennemi caché
    public void UnregisterHiddenEnemy()            => _hiddenEnemy  = null;           // Libère le slot ennemi
    public bool CanHideEnemy()                     => _hiddenEnemy == null && _state != SearchState.Searched; // Vérifie si le bureau est libre

    private void CheckPlayerProximity() // Met à jour l'état selon la zone de fouille
    {
        if (_state == SearchState.Searched || _state == SearchState.Searching) return;

        bool isInRange = IsPlayerInSearchZone();

        if (isInRange && _state == SearchState.Idle) // Joueur entre dans la zone
        {
            _state = SearchState.PlayerNearby;
            OnPlayerEnterRange.Invoke(this);
        }
        else if (!isInRange && _state == SearchState.PlayerNearby) // Joueur quitte la zone
        {
            _state = SearchState.Idle;
            OnPlayerExitRange.Invoke(this);
        }
    }

    private bool IsPlayerInSearchZone() // Vérifie si le joueur est dans la zone rectangulaire étendue
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        Vector2 halfExtents = col != null
            ? (Vector2)(col.bounds.extents + new Vector3(_searchMargin, _searchMargin, 0f))
            : new Vector2(_searchMargin, _searchMargin);

        Vector2 delta = (Vector2)_playerTransform.position - (Vector2)transform.position;
        return Mathf.Abs(delta.x) <= halfExtents.x && Mathf.Abs(delta.y) <= halfExtents.y;
    }

    private IEnumerator SearchCoroutine() // Attend la durée, génère le loot et notifie
    {
        float elapsed              = 0f;
        const float updateInterval = 0.1f;
        float timeSinceLastUpdate  = 0f;

        while (elapsed < _searchDuration) // Attend la durée complète de fouille
        {
            elapsed             += Time.deltaTime;
            timeSinceLastUpdate += Time.deltaTime;

            if (timeSinceLastUpdate >= updateInterval) // Envoie la progression toutes les 0.1s
            {
                timeSinceLastUpdate = 0f;
                OnSearchProgressUpdate.Invoke(Mathf.Clamp01(elapsed / _searchDuration));
            }
            yield return null;
        }

        OnSearchProgressUpdate.Invoke(1f);

        if (_lootDropper != null)
            _lootDropper.DropLoot(transform.position);

        _state           = SearchState.Searched;
        _searchCoroutine = null;
        OnSearchComplete.Invoke();

        if (_networkEnemy != null && !_networkEnemy.gameObject.activeSelf) // Déclenche l'ennemi réseau si associé
            _networkEnemy.TriggerFromDesk(this);

        if (_hiddenEnemy != null) // Révèle l'ennemi caché si présent
        {
            EnemyHidden toReveal = _hiddenEnemy;
            _hiddenEnemy = null;
            toReveal.RevealFromDesk();
        }
        else if (_enemyHiddenPrefab != null && Random.value < _infestedChance) // Spawn infesté aléatoire
        {
            GameObject go      = Instantiate(_enemyHiddenPrefab, transform.position, Quaternion.identity);
            EnemyHidden spawned = go.GetComponent<EnemyHidden>();
            if (spawned != null) spawned.SpawnAndAttack(transform.position);
        }

        OnPlayerExitRange.Invoke(this);
    }
}
