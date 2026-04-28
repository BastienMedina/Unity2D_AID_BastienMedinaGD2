using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SI_WaveManager : MonoBehaviour
{
    [SerializeField] private GameObject _virusFastPrefab;
    [SerializeField] private GameObject _virusHeavyPrefab;
    [SerializeField] private GameObject _virusShooterPrefab;
    [SerializeField] private float _spawnIntervalStart = 2.0f;
    [SerializeField] private float _spawnIntervalMin = 0.5f;
    [SerializeField] private float _spawnIntervalDecay = 0.01f;
    [SerializeField] private float _spawnXMin = -2.5f;
    [SerializeField] private float _spawnXMax = 2.5f;
    [SerializeField] private float _spawnY = 3.5f;
    [SerializeField] private float _phase2Time = 30f;
    [SerializeField] private float _phase3Time = 60f;
    [SerializeField] private UnityEvent<GameObject> _onVirusSpawned;
    [SerializeField] private AudioClip _spawnClip;
    [SerializeField] private AudioClip _phaseChangeClip;

    public UnityEvent<GameObject> OnVirusSpawned => _onVirusSpawned; // Expose l'événement de spawn pour l'injecteur

    [HideInInspector] public GameObject VirusFastPrefab    => _virusFastPrefab;
    [HideInInspector] public GameObject VirusHeavyPrefab   => _virusHeavyPrefab;
    [HideInInspector] public GameObject VirusShooterPrefab => _virusShooterPrefab;

    private float _elapsed;
    private int _currentPhase = 1;

    private void Start() // Valide les prefabs et démarre la boucle de spawn
    {
        Debug.Log($"[SI] WaveManager — VirusFast={_virusFastPrefab != null} | VirusHeavy={_virusHeavyPrefab != null} | VirusShooter={_virusShooterPrefab != null}");
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop() // Boucle infinie de spawn progressif d'ennemis
    {
        _elapsed = 0f;
        while (true)
        {
            float interval = Mathf.Max(_spawnIntervalMin, _spawnIntervalStart - _elapsed * _spawnIntervalDecay);
            yield return new WaitForSecondsRealtime(interval); // Ignore Time.timeScale pour la pause

            GameObject prefab = PickPrefab(_elapsed);
            if (prefab == null) { _elapsed += interval; continue; }

            GameObject virusObject = Instantiate(prefab, new Vector3(Random.Range(_spawnXMin, _spawnXMax), _spawnY, 0f), Quaternion.identity);
            if (virusObject == null) { Debug.LogError("[SI_WaveManager] Instanciation échouée.", this); _elapsed += interval; continue; }

            AudioManager.Instance?.PlaySFX(_spawnClip);
            _onVirusSpawned?.Invoke(virusObject);

            int newPhase = GetCurrentWave();
            if (newPhase != _currentPhase) // Joue le son de changement de phase
            {
                _currentPhase = newPhase;
                AudioManager.Instance?.PlaySFX(_phaseChangeClip);
            }

            _elapsed += interval;
        }
    }

    private GameObject PickPrefab(float elapsed) // Sélectionne le prefab selon la phase courante
    {
        if (elapsed < _phase2Time) return _virusFastPrefab; // Phase 1 : uniquement VirusFast

        if (elapsed < _phase3Time) // Phase 2 : VirusFast ou VirusHeavy à parts égales
            return Random.value < 0.5f ? _virusFastPrefab : _virusHeavyPrefab;

        float roll = Random.value; // Phase 3 : trois types avec poids différenciés
        if (roll < 0.5f) return _virusFastPrefab;
        if (roll < 0.8f) return _virusHeavyPrefab;
        return _virusShooterPrefab;
    }

    public int GetCurrentWave() // Retourne la phase courante selon le temps écoulé
    {
        if (_elapsed < _phase2Time) return 1;
        if (_elapsed < _phase3Time) return 2;
        return 3;
    }
}
