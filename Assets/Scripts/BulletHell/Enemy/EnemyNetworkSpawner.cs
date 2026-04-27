using UnityEngine;

// Gère les points de spawn écran et le cycle d'apparition des unités
public class EnemyNetworkSpawner : MonoBehaviour
{
    // Positions des écrans depuis lesquels les unités apparaissent
    [SerializeField] private Transform[] _screenSpawnPoints;

    // Préfab de l'unité réseau instanciée à chaque cycle de spawn
    [SerializeField] private GameObject _networkEnemyPrefab;

    // Délai en secondes entre deux tentatives de spawn d'unité
    [SerializeField] private float _spawnInterval = 4f;

    // Nombre maximum d'unités actives simultanément dans la salle
    [SerializeField] private int _maxActiveAtOnce = 2;

    // Vitesse de déplacement transmise à chaque unité spawnée
    [SerializeField] private float _unitSpeed = 2.5f;

    // Dégâts transmis à chaque unité réseau lors de son initialisation
    [SerializeField] private int _unitDamage = 1;

    // Référence au gestionnaire de vies du joueur pour les unités
    [SerializeField] private LivesManager _livesManager;

    // Référence au Transform du joueur transmise aux unités spawnées
    [SerializeField] private Transform _playerTransform;

    // Rayon au-delà duquel une unité est considérée hors de la salle
    [SerializeField] private float _despawnRadius = 10f;

    // Son joué lors du spawn d'une unité réseau
    [SerializeField] private AudioClip _spawnClip;

    // Nombre d'unités actuellement actives dans la salle
    private int _activeUnitCount = 0;

    // Indique si le cycle de spawn est actuellement en cours
    private bool _isSpawning = false;

    // Démarre le cycle de spawn quand le joueur entre dans le trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Vérifie que c'est bien le joueur qui entre dans la zone
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // Ignore si le cycle de spawn est déjà actif
        if (_isSpawning)
        {
            return;
        }

        // Active le drapeau pour éviter les démarrages multiples
        _isSpawning = true;

        // Lance le cycle de spawn à intervalle régulier configuré
        InvokeRepeating(nameof(TrySpawnUnit), 0f, _spawnInterval);
    }

    // Stoppe le cycle de spawn quand le joueur quitte la zone
    private void OnTriggerExit2D(Collider2D other)
    {
        // Vérifie que c'est bien le joueur qui quitte la zone
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // Arrête le cycle de spawn et réinitialise le drapeau
        StopSpawning();
    }

    // Tente d'instancier une unité si le quota n'est pas atteint
    private void TrySpawnUnit()
    {
        // Bloque le spawn si le nombre maximum d'unités est atteint
        if (_activeUnitCount >= _maxActiveAtOnce)
        {
            return;
        }

        // Bloque le spawn si aucun point de spawn n'est configuré
        if (_screenSpawnPoints == null || _screenSpawnPoints.Length == 0)
        {
            return;
        }

        // Sélectionne un point de spawn aléatoire parmi les écrans
        int randomIndex = Random.Range(0, _screenSpawnPoints.Length);
        Transform spawnPoint = _screenSpawnPoints[randomIndex];

        // Instancie l'unité réseau au point de spawn sélectionné
        GameObject unitObject = Instantiate(_networkEnemyPrefab, spawnPoint.position, Quaternion.identity);

        // Récupère le script EnemyNetworkUnit sur l'objet instancié
        EnemyNetworkUnit unit = unitObject.GetComponent<EnemyNetworkUnit>();

        // Initialise l'unité avec toutes les données nécessaires
        unit.Initialize(_playerTransform, _livesManager, this, _unitSpeed, _unitDamage, _despawnRadius);

        AudioManager.Instance?.PlaySFX(_spawnClip);

        // Incrémente le compteur d'unités actives après le spawn
        _activeUnitCount++;
    }

    // Décrémente le compteur quand une unité meurt ou se despawn
    public void NotifyUnitRemoved()
    {
        // Réduit le nombre d'unités actives d'une unité retirée
        _activeUnitCount--;

        // Empêche le compteur de descendre en dessous de zéro
        if (_activeUnitCount < 0)
        {
            _activeUnitCount = 0;
        }
    }

    // Arrête le cycle de spawn et réinitialise l'état interne
    private void StopSpawning()
    {
        // Annule tous les appels répétés en cours liés au spawn
        CancelInvoke(nameof(TrySpawnUnit));

        // Marque le cycle de spawn comme inactif
        _isSpawning = false;
    }
}
