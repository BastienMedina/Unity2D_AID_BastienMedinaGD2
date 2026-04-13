using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Gère la proximité joueur, la fouille et le butin.
public class SearchableObject : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Énumération des états internes possibles
    // -------------------------------------------------------------------------

    // Représente les quatre états possibles de cet objet.
    public enum SearchState
    {
        Idle,
        PlayerNearby,
        Searching,
        Searched
    }

    // -------------------------------------------------------------------------
    // Paramètres configurables
    // -------------------------------------------------------------------------

    // Distance maximale pour détecter la présence du joueur.
    [SerializeField] private float _searchRadius = 1.5f;

    // Durée en secondes nécessaire pour fouiller cet objet.
    [SerializeField] private float _searchDuration = 1.5f;

    // Référence au composant qui génère le butin à la fouille.
    [SerializeField] private LootDropper _lootDropper;

    // Ennemi réseau lié à ce bureau fouillable
    [SerializeField] private DeskNetworkEnemy _networkEnemy;

    // Étiquette textuelle identifiant cet objet dans l'interface.
    [SerializeField] private string _objectLabel = "Bureau";

    // Intervalle en secondes entre chaque vérification de distance.
    [SerializeField] private float _proximityCheckInterval = 0.2f;

    // -------------------------------------------------------------------------
    // Système infesté
    // -------------------------------------------------------------------------

    // Probabilité (0-1) que ce bureau contienne un ennemi caché
    [SerializeField] [Range(0f, 1f)] private float _infestedChance = 0f;

    // Prefab EnemyHidden à instancier si ce bureau est infesté
    [SerializeField] private GameObject _enemyHiddenPrefab;

    // Ennemi caché enregistré dans ce bureau (arrive après une attaque)
    private EnemyHidden _hiddenEnemy;

    // -------------------------------------------------------------------------
    // Événements publics
    // -------------------------------------------------------------------------

    // Déclenché au début de la fouille par le joueur.
    public UnityEvent OnSearchStarted = new UnityEvent();

    // Déclenché quand le joueur entre dans le rayon de fouille.
    public UnityEvent<SearchableObject> OnPlayerEnterRange = new UnityEvent<SearchableObject>();

    // Déclenché quand le joueur quitte le rayon de fouille.
    public UnityEvent<SearchableObject> OnPlayerExitRange = new UnityEvent<SearchableObject>();

    // Déclenché quand la fouille se termine avec succès.
    public UnityEvent OnSearchComplete = new UnityEvent();

    // Déclenché périodiquement avec la progression de 0 à 1.
    public UnityEvent<float> OnSearchProgressUpdate = new UnityEvent<float>();

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    // État courant de cet objet dans la machine à états.
    private SearchState _state = SearchState.Idle;

    // Référence au Transform du joueur pour le calcul de distance.
    private Transform _playerTransform;

    // Référence à la coroutine de fouille pour l'annulation.
    private Coroutine _searchCoroutine;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Localise le joueur et démarre la vérification périodique.
    private void Awake()
    {
        // Cherche le joueur par tag pour éviter une dépendance directe.
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
            return;

        // Stocke le Transform du joueur pour les calculs de distance.
        _playerTransform = playerObject.transform;
    }

    // Démarre la vérification de proximité répétée par interval.
    private void Start()
    {
        // Ignore le démarrage si le joueur n'a pas été localisé.
        if (_playerTransform == null)
            return;

        // Démarre la vérification périodique de la distance joueur.
        InvokeRepeating(nameof(CheckPlayerProximity), 0f, _proximityCheckInterval);
    }

    // Arrête proprement la vérification quand l'objet est détruit.
    private void OnDestroy()
    {
        // Annule tous les InvokeRepeating actifs sur ce composant.
        CancelInvoke(nameof(CheckPlayerProximity));
    }

    // -------------------------------------------------------------------------
    // API publique
    // -------------------------------------------------------------------------

    /// <summary>Démarre la fouille si le joueur est proche et l'objet non fouillé.</summary>
    public void StartSearch()
    {
        // Ignore si l'objet a déjà été fouillé entièrement.
        if (_state == SearchState.Searched)
            return;

        // Ignore si le joueur n'est pas dans le rayon requis.
        if (_state != SearchState.PlayerNearby)
            return;

        // Passe à l'état Searching et démarre la coroutine.
        _state = SearchState.Searching;

        // Déclenché au début de la fouille.
        OnSearchStarted.Invoke();

        // Lance la coroutine et conserve la référence pour annulation.
        _searchCoroutine = StartCoroutine(SearchCoroutine());
    }

    /// <summary>Annule la fouille en cours et revient à PlayerNearby.</summary>
    public void CancelSearch()
    {
        // Ignore si aucune fouille n'est actuellement en cours.
        if (_state != SearchState.Searching)
            return;

        // Arrête la coroutine de fouille si elle est active.
        if (_searchCoroutine != null)
        {
            // Interrompt la coroutine et libère la référence.
            StopCoroutine(_searchCoroutine);
            _searchCoroutine = null;
        }

        // Retourne à l'état PlayerNearby après annulation.
        _state = SearchState.PlayerNearby;
    }

    /// <summary>Retourne vrai si l'objet a déjà été fouillé.</summary>
    public bool IsSearched()
    {
        // Compare l'état courant à l'état terminal Searched.
        return _state == SearchState.Searched;
    }

    /// <summary>Retourne l'état courant de cet objet fouillable.</summary>
    public SearchState GetState()
    {
        // Expose l'état interne en lecture seule pour les systèmes.
        return _state;
    }

    /// <summary>Retourne l'étiquette textuelle de cet objet fouillable.</summary>
    public string GetLabel()
    {
        // Expose l'étiquette configurée pour l'affichage externe.
        return _objectLabel;
    }

    /// <summary>Définit l'étiquette textuelle de cet objet fouillable depuis l'extérieur.</summary>
    public void SetLabel(string label)
    {
        _objectLabel = label;
    }

    /// <summary>Assigne le LootDropper depuis l'extérieur (câblage runtime).</summary>
    public void SetLootDropper(LootDropper lootDropper)
    {
        _lootDropper = lootDropper;
    }

    /// <summary>Configure la probabilité d'infestation et le prefab ennemi depuis ProceduralMapGenerator.</summary>
    public void SetInfested(float chance, GameObject enemyHiddenPrefab)
    {
        _infestedChance    = chance;
        _enemyHiddenPrefab = enemyHiddenPrefab;
    }

    /// <summary>Enregistre un EnemyHidden qui se cache dans ce bureau.</summary>
    public void RegisterHiddenEnemy(EnemyHidden enemy)
    {
        _hiddenEnemy = enemy;
    }

    /// <summary>Libère le slot de bureau (appelé à la mort de l'ennemi).</summary>
    public void UnregisterHiddenEnemy()
    {
        _hiddenEnemy = null;
    }

    /// <summary>Retourne vrai si ce bureau peut accueillir un ennemi caché.</summary>
    public bool CanHideEnemy()
    {
        return _hiddenEnemy == null && _state != SearchState.Searched;
    }

    // -------------------------------------------------------------------------
    // Vérification de proximité
    // -------------------------------------------------------------------------

    // Évalue la distance joueur et met à jour l'état si nécessaire.
    private void CheckPlayerProximity()
    {
        // Ignore la vérification si la fouille est terminée.
        if (_state == SearchState.Searched)
            return;

        // Ignore si une fouille est actuellement en cours.
        if (_state == SearchState.Searching)
            return;

        // Calcule la distance entre cet objet et le joueur.
        float distance = Vector2.Distance(transform.position, _playerTransform.position);

        // Détermine si le joueur est dans le rayon de fouille.
        bool isInRange = distance <= _searchRadius;

        // Traite l'entrée dans le rayon depuis l'état Idle.
        if (isInRange && _state == SearchState.Idle)
        {
            // Passe à l'état PlayerNearby et notifie les abonnés.
            _state = SearchState.PlayerNearby;

            // Notifie les abonnés avec une référence à cet objet.
            OnPlayerEnterRange.Invoke(this);
        }

        // Traite la sortie du rayon depuis l'état PlayerNearby.
        else if (!isInRange && _state == SearchState.PlayerNearby)
        {
            // Retourne à l'état Idle et notifie les abonnés.
            _state = SearchState.Idle;

            // Notifie les abonnés avec une référence à cet objet.
            OnPlayerExitRange.Invoke(this);
        }
    }

    // -------------------------------------------------------------------------
    // Coroutine de fouille
    // -------------------------------------------------------------------------

    // Attend la durée configurée, génère le butin et notifie.
    private IEnumerator SearchCoroutine()
    {
        // Initialise le temps écoulé depuis le début de la fouille.
        float elapsed = 0f;

        // Intervalle fixe entre chaque mise à jour de progression.
        const float progressUpdateInterval = 0.1f;

        // Accumule le temps depuis la dernière mise à jour.
        float timeSinceLastUpdate = 0f;

        // Boucle jusqu'à ce que la durée de fouille soit atteinte.
        while (elapsed < _searchDuration)
        {
            // Incrémente le temps écoulé depuis le dernier frame.
            elapsed += Time.deltaTime;

            // Incrémente le temps depuis la dernière mise à jour.
            timeSinceLastUpdate += Time.deltaTime;

            // Envoie une mise à jour de progression toutes les 0.1s.
            if (timeSinceLastUpdate >= progressUpdateInterval)
            {
                // Remet l'accumulateur à zéro après chaque mise à jour.
                timeSinceLastUpdate = 0f;

                // Calcule et clamp la progression entre 0 et 1.
                float progress = Mathf.Clamp01(elapsed / _searchDuration);

                // Notifie les abonnés avec la progression courante.
                OnSearchProgressUpdate.Invoke(progress);
            }

            // Attend le prochain frame avant de continuer.
            yield return null;
        }

        // Envoie la progression finale à 1 avant la complétion.
        OnSearchProgressUpdate.Invoke(1f);

        // Génère le butin à la position de cet objet si possible.
        if (_lootDropper != null)
            _lootDropper.DropLoot(transform.position);

        // Passe à l'état terminal après la fouille complète.
        _state = SearchState.Searched;

        // Libère la référence coroutine après complétion normale.
        _searchCoroutine = null;

        // Notifie les abonnés de la complétion de la fouille.
        OnSearchComplete.Invoke();

        // Déclenche l'ennemi réseau si assigné à ce bureau
        if (_networkEnemy != null && !_networkEnemy.gameObject.activeSelf)
            _networkEnemy.TriggerFromDesk(this);

        // Si un ennemi caché s'est réfugié dans ce bureau, le révèle
        if (_hiddenEnemy != null)
        {
            EnemyHidden toReveal = _hiddenEnemy;
            _hiddenEnemy = null;
            toReveal.RevealFromDesk();
        }
        // Sinon, tente un spawn infesté aléatoire
        else if (_enemyHiddenPrefab != null && Random.value < _infestedChance)
        {
            GameObject go = Instantiate(_enemyHiddenPrefab, transform.position, Quaternion.identity);
            EnemyHidden spawned = go.GetComponent<EnemyHidden>();
            if (spawned != null)
                spawned.SpawnAndAttack(transform.position);
        }

        // Masque le bouton en notifiant la sortie de proximité.
        OnPlayerExitRange.Invoke(this);
    }
}
