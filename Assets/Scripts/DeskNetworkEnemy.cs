using System.Collections;
using UnityEngine;

// Gère l'ennemi réseau entre les bureaux de la scène
public class DeskNetworkEnemy : EnemyBase
{
    // ── RÉFÉRENCES ───────────────────────────────────

    // Transform du joueur pour cibler l'attaque
    [SerializeField] private Transform _playerTransform;

    // Référence au gestionnaire de vies du joueur
    [SerializeField] private LivesManager _livesManager;

    // Tous les bureaux fouillables de la scène
    [SerializeField] private SearchableObject[] _allDesks;

    // ── PARAMÈTRES ───────────────────────────────────

    // Durée de la pause après émergence avant attaque
    [SerializeField] private float _emergeDelay = 0.5f;

    // Durée visible du laser électrique
    [SerializeField] private float _laserDuration = 0.4f;

    // Dégâts infligés par le laser électrique
    [SerializeField] private int _laserDamage = 1;

    // Délai avant de replonger dans le bureau
    [SerializeField] private float _retreatDelay = 0.8f;

    // Délai avant de réapparaître dans un autre bureau
    [SerializeField] private float _respawnDelay = 2f;

    // ── ÉTATS ────────────────────────────────────────

    // États possibles de l'ennemi réseau
    private enum NetworkState { Hidden, Emerging, Attacking, Retreating }

    // État courant de l'ennemi réseau
    private NetworkState _state = NetworkState.Hidden;

    // ── ÉTAT INTERNE ─────────────────────────────────

    // Bureau actuel d'où l'ennemi est sorti
    private SearchableObject _currentDesk;

    // ── COMPOSANTS ───────────────────────────────────

    // LineRenderer pour afficher le laser électrique
    private LineRenderer _lineRenderer;

    // ── INITIALISATION ───────────────────────────────

    // Initialise le LineRenderer et cache l'ennemi
    protected override void Awake()
    {
        // Appelle l'initialisation de santé définie dans EnemyBase
        base.Awake();

        // Ajoute ou récupère le LineRenderer sur cet objet
        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer == null)
            _lineRenderer = gameObject.AddComponent<LineRenderer>();

        // Fixe le nombre de points du trait laser
        _lineRenderer.positionCount = 2;

        // Configure l'épaisseur du faisceau laser
        _lineRenderer.startWidth = 0.08f;
        _lineRenderer.endWidth   = 0.02f;

        // Applique un matériau compatible URP sans texture
        _lineRenderer.material = new Material(
            Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));

        // Couleur cyan au départ du laser
        _lineRenderer.startColor = new Color(0.4f, 0.9f, 1f, 1f);

        // Couleur blanche transparente à l'arrivée du laser
        _lineRenderer.endColor = new Color(1f, 1f, 1f, 0.6f);

        // Cache le laser au démarrage
        _lineRenderer.enabled = false;

        // Cache l'ennemi entier au démarrage de la scène
        gameObject.SetActive(false);
    }

    // ── DÉCLENCHEMENT DEPUIS UN BUREAU ───────────────

    // Déclenche l'émergence depuis un bureau fouillé
    public void TriggerFromDesk(SearchableObject desk)
    {
        // Ignore si l'ennemi est déjà actif dans la scène
        if (_state != NetworkState.Hidden) return;

        // Ignore si l'ennemi est mort et ne doit plus agir
        if (IsDead()) return;

        // Enregistre le bureau source de cette émergence
        _currentDesk = desk;

        // Téléporte l'ennemi à la position du bureau
        transform.position = desk.transform.position;

        // Active l'ennemi dans la scène avant la coroutine
        gameObject.SetActive(true);

        // Lance la séquence complète d'émergence et d'attaque
        StartCoroutine(EmergeAndAttack());
    }

    // ── SÉQUENCE PRINCIPALE ───────────────────────────

    // Gère l'émergence, l'attaque et la retraite
    private IEnumerator EmergeAndAttack()
    {
        // Passe en état d'émergence visuelle depuis le bureau
        _state = NetworkState.Emerging;

        // Attend la fin du délai d'émergence configuré
        yield return new WaitForSeconds(_emergeDelay);

        // Passe en état d'attaque laser vers le joueur
        _state = NetworkState.Attacking;

        // Tire le laser électrique et attend sa fin
        yield return StartCoroutine(FireElectricLaser());

        // Passe en état de retraite vers un autre bureau
        _state = NetworkState.Retreating;

        // Attend avant de replonger dans le réseau
        yield return new WaitForSeconds(_retreatDelay);

        // Lance la retraite vers le bureau suivant
        yield return StartCoroutine(RetreatToNextDesk());
    }

    // ── LASER ÉLECTRIQUE ─────────────────────────────

    // Tire un laser électrique vers le joueur
    private IEnumerator FireElectricLaser()
    {
        // Abandonne si la référence joueur est manquante
        if (_playerTransform == null) yield break;

        // Calcule les positions source et cible du laser
        Vector3 origin    = transform.position;
        Vector3 targetPos = _playerTransform.position;

        // Positionne les deux extrémités du laser
        _lineRenderer.SetPosition(0, origin);
        _lineRenderer.SetPosition(1, targetPos);

        // Active le LineRenderer pour afficher le faisceau
        _lineRenderer.enabled = true;

        // Calcule la direction normalisée vers le joueur
        Vector2 direction = ((Vector2)targetPos - (Vector2)origin).normalized;

        // Calcule la distance entre l'ennemi et le joueur
        float distance = Vector2.Distance(origin, targetPos);

        // Lance un raycast vers le joueur sans filtre de layer
        RaycastHit2D hit = Physics2D.Raycast(
            origin, direction, distance);

        // Applique les dégâts si le collider touché est le joueur
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            // Inflige les dégâts configurés via le LivesManager
            _livesManager.TakeDamage();
            Debug.Log("[NETWORK] Laser électrique touche le joueur");
        }

        // Fait scintiller le laser pendant la durée configurée
        yield return StartCoroutine(FlickerLaser());

        // Cache le laser après la fin du scintillement
        _lineRenderer.enabled = false;
    }

    // Fait scintiller le laser pour un effet électrique
    private IEnumerator FlickerLaser()
    {
        // Durée écoulée depuis le début du scintillement
        float elapsed = 0f;

        // Durée d'un demi-cycle de clignotement en secondes
        const float flickerStep = 0.06f;

        // Fait clignoter le laser pendant la durée configurée
        while (elapsed < _laserDuration)
        {
            // Inverse l'état du laser pour le clignotement
            _lineRenderer.enabled = !_lineRenderer.enabled;

            // Ajoute un décalage aléatoire à la position cible
            if (_playerTransform != null)
            {
                // Calcule une position jitter autour du joueur
                Vector3 jitter = _playerTransform.position + new Vector3(
                    Random.Range(-0.15f, 0.15f),
                    Random.Range(-0.15f, 0.15f),
                    0f);

                // Met à jour l'extrémité du laser avec le jitter
                _lineRenderer.SetPosition(1, jitter);
            }

            // Attend un court instant avant le prochain clignotement
            yield return new WaitForSeconds(flickerStep);

            // Incrémente le temps écoulé du clignotement
            elapsed += flickerStep;
        }

        // S'assure que le laser est bien caché en fin de boucle
        _lineRenderer.enabled = false;
    }

    // ── RETRAITE VERS UN AUTRE BUREAU ────────────────

    // Téléporte l'ennemi vers un autre bureau de la scène
    private IEnumerator RetreatToNextDesk()
    {
        // Cache l'ennemi pendant le transit réseau simulé
        gameObject.SetActive(false);

        // Repasse en état caché pendant le délai de respawn
        _state = NetworkState.Hidden;

        // Attend le délai de respawn avant de réapparaître
        yield return new WaitForSeconds(_respawnDelay);

        // Abandonne si l'ennemi est mort pendant le délai
        if (IsDead()) yield break;

        // Cherche le bureau le plus proche différent de l'actuel
        SearchableObject nextDesk = FindNextDesk();

        // Abandonne si aucun autre bureau n'est disponible
        if (nextDesk == null)
        {
            // Reste caché définitivement sans autre bureau
            yield break;
        }

        // Réémerge depuis le nouveau bureau trouvé
        TriggerFromDesk(nextDesk);
    }

    // Trouve le bureau le plus proche différent du bureau actuel
    private SearchableObject FindNextDesk()
    {
        // Bureau le plus proche trouvé jusqu'ici
        SearchableObject nearest = null;

        // Distance minimale trouvée, initialisée au maximum
        float minDist = float.MaxValue;

        // Parcourt tous les bureaux de la liste
        foreach (SearchableObject desk in _allDesks)
        {
            // Ignore le bureau actuel de l'ennemi
            if (desk == _currentDesk) continue;

            // Ignore les bureaux non assignés dans le tableau
            if (desk == null) continue;

            // Calcule la distance entre l'ennemi et le bureau
            float dist = Vector2.Distance(
                transform.position, desk.transform.position);

            // Met à jour le plus proche si la distance est moindre
            if (dist < minDist)
            {
                minDist = dist;
                nearest = desk;
            }
        }

        // Retourne le bureau le plus proche trouvé
        return nearest;
    }

    // ── MORT DE L'ENNEMI ─────────────────────────────

    // Gère la mort de l'ennemi réseau définitivement
    protected override void HandleDeath()
    {
        // Arrête toutes les coroutines en cours sur cet objet
        StopAllCoroutines();

        // Cache le laser si celui-ci est encore affiché
        _lineRenderer.enabled = false;

        // Désactive l'objet entier pour le retirer de la scène
        gameObject.SetActive(false);
    }
}
