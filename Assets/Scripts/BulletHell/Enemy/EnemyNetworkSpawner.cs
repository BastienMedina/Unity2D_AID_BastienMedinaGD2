using UnityEngine;

public class EnemyNetworkSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] _screenSpawnPoints;
    [SerializeField] private GameObject _networkEnemyPrefab;
    [SerializeField] private float _spawnInterval = 4f;
    [SerializeField] private int _maxActiveAtOnce = 2;
    [SerializeField] private float _unitSpeed = 2.5f;
    [SerializeField] private int _unitDamage = 1;
    [SerializeField] private LivesManager _livesManager;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private float _despawnRadius = 10f;
    [SerializeField] private AudioClip _spawnClip;

    private int _activeUnitCount;
    private bool _isSpawning;

    private void OnTriggerEnter2D(Collider2D other) // Démarre le spawn quand le joueur entre
    {
        if (!other.CompareTag("Player") || _isSpawning)
            return;

        _isSpawning = true;
        InvokeRepeating(nameof(TrySpawnUnit), 0f, _spawnInterval);
    }

    private void OnTriggerExit2D(Collider2D other) // Stoppe le spawn quand le joueur part
    {
        if (!other.CompareTag("Player"))
            return;

        StopSpawning();
    }

    private void TrySpawnUnit() // Instancie une unité si le quota n'est pas atteint
    {
        if (_activeUnitCount >= _maxActiveAtOnce)
            return;

        if (_screenSpawnPoints == null || _screenSpawnPoints.Length == 0)
            return;

        Transform spawnPoint  = _screenSpawnPoints[Random.Range(0, _screenSpawnPoints.Length)];
        GameObject unitObject = Instantiate(_networkEnemyPrefab, spawnPoint.position, Quaternion.identity);
        EnemyNetworkUnit unit = unitObject.GetComponent<EnemyNetworkUnit>();
        unit.Initialize(_playerTransform, _livesManager, this, _unitSpeed, _unitDamage, _despawnRadius);
        AudioManager.Instance?.PlaySFX(_spawnClip);
        _activeUnitCount++;
    }

    public void NotifyUnitRemoved() // Décrémente le compteur d'unités actives
    {
        _activeUnitCount = Mathf.Max(0, _activeUnitCount - 1);
    }

    private void StopSpawning() // Annule le cycle de spawn et réinitialise l'état
    {
        CancelInvoke(nameof(TrySpawnUnit));
        _isSpawning = false;
    }
}
