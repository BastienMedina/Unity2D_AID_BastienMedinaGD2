using System.Collections;
using UnityEngine;

public class DeskNetworkEnemy : EnemyBase
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private LivesManager _livesManager;
    [SerializeField] private SearchableObject[] _allDesks;
    [SerializeField] private float _emergeDelay = 0.5f;
    [SerializeField] private float _laserDuration = 0.4f;
    [SerializeField] private int _laserDamage = 1;
    [SerializeField] private float _retreatDelay = 0.8f;
    [SerializeField] private float _respawnDelay = 2f;

    private enum NetworkState { Hidden, Emerging, Attacking, Retreating }
    private NetworkState _state = NetworkState.Hidden;
    private SearchableObject _currentDesk;
    private LineRenderer _lineRenderer;

    protected override void Awake() // Initialise le LineRenderer et cache l'ennemi
    {
        base.Awake();

        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer == null)
            _lineRenderer = gameObject.AddComponent<LineRenderer>();

        _lineRenderer.positionCount = 2;
        _lineRenderer.startWidth = 0.08f;
        _lineRenderer.endWidth   = 0.02f;
        _lineRenderer.material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
        _lineRenderer.startColor = new Color(0.4f, 0.9f, 1f, 1f);
        _lineRenderer.endColor = new Color(1f, 1f, 1f, 0.6f);
        _lineRenderer.enabled = false;

        gameObject.SetActive(false);
    }

    public void TriggerFromDesk(SearchableObject desk) // Déclenche l'émergence depuis un bureau
    {
        if (_state != NetworkState.Hidden) return;
        if (IsDead()) return;

        _currentDesk = desk;
        transform.position = desk.transform.position;
        gameObject.SetActive(true);
        StartCoroutine(EmergeAndAttack());
    }

    private IEnumerator EmergeAndAttack() // Gère l'émergence, l'attaque et la retraite
    {
        _state = NetworkState.Emerging;
        yield return new WaitForSeconds(_emergeDelay);

        _state = NetworkState.Attacking;
        yield return StartCoroutine(FireElectricLaser());

        _state = NetworkState.Retreating;
        yield return new WaitForSeconds(_retreatDelay);

        yield return StartCoroutine(RetreatToNextDesk());
    }

    private IEnumerator FireElectricLaser() // Tire un laser électrique vers le joueur
    {
        if (_playerTransform == null) yield break;

        Vector3 origin    = transform.position;
        Vector3 targetPos = _playerTransform.position;

        _lineRenderer.SetPosition(0, origin);
        _lineRenderer.SetPosition(1, targetPos);
        _lineRenderer.enabled = true;

        Vector2 direction = ((Vector2)targetPos - (Vector2)origin).normalized;
        float distance = Vector2.Distance(origin, targetPos);
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            _livesManager.TakeDamage();
            Debug.Log("[NETWORK] Laser électrique touche le joueur");
        }

        yield return StartCoroutine(FlickerLaser());
        _lineRenderer.enabled = false;
    }

    private IEnumerator FlickerLaser() // Fait scintiller le laser pour un effet électrique
    {
        float elapsed = 0f;
        const float flickerStep = 0.06f;

        while (elapsed < _laserDuration) // Clignote jusqu'à la fin de la durée
        {
            _lineRenderer.enabled = !_lineRenderer.enabled;

            if (_playerTransform != null)
            {
                Vector3 jitter = _playerTransform.position + new Vector3( // Ajoute un jitter aléatoire
                    Random.Range(-0.15f, 0.15f),
                    Random.Range(-0.15f, 0.15f),
                    0f);
                _lineRenderer.SetPosition(1, jitter);
            }

            yield return new WaitForSeconds(flickerStep);
            elapsed += flickerStep;
        }

        _lineRenderer.enabled = false;
    }

    private IEnumerator RetreatToNextDesk() // Téléporte l'ennemi vers un autre bureau
    {
        gameObject.SetActive(false);
        _state = NetworkState.Hidden;
        yield return new WaitForSeconds(_respawnDelay);

        if (IsDead()) yield break;

        SearchableObject nextDesk = FindNextDesk();
        if (nextDesk == null) yield break;

        TriggerFromDesk(nextDesk);
    }

    private SearchableObject FindNextDesk() // Retourne le bureau le plus proche différent de l'actuel
    {
        SearchableObject nearest = null;
        float minDist = float.MaxValue;

        foreach (SearchableObject desk in _allDesks) // Parcourt tous les bureaux disponibles
        {
            if (desk == _currentDesk) continue;
            if (desk == null) continue;

            float dist = Vector2.Distance(transform.position, desk.transform.position);
            if (dist < minDist) { minDist = dist; nearest = desk; }
        }

        return nearest;
    }

    protected override void HandleDeath() // Arrête les coroutines et désactive l'ennemi
    {
        StopAllCoroutines();
        _lineRenderer.enabled = false;
        gameObject.SetActive(false);
    }
}
