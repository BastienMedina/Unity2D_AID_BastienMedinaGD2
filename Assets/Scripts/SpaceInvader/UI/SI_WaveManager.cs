using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Gère le spawn continu et progressif des ennemis en solo
public class SI_WaveManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Prefabs des trois types de virus disponibles au spawn
    // -------------------------------------------------------------------------

    // Prefab du virus rapide disponible dès la phase 1
    [SerializeField] private GameObject _virusFastPrefab;

    // Prefab du virus lourd débloqué en phase 2
    [SerializeField] private GameObject _virusHeavyPrefab;

    // Prefab du virus tireur débloqué en phase 3
    [SerializeField] private GameObject _virusShooterPrefab;

    // -------------------------------------------------------------------------
    // Paramètres de l'intervalle de spawn décroissant
    // -------------------------------------------------------------------------

    // Intervalle initial entre deux spawns en secondes
    [SerializeField] private float _spawnIntervalStart = 2.0f;

    // Intervalle minimum entre deux spawns en secondes
    [SerializeField] private float _spawnIntervalMin = 0.5f;

    // Réduction de l'intervalle par seconde écoulée
    [SerializeField] private float _spawnIntervalDecay = 0.01f;

    // -------------------------------------------------------------------------
    // Paramètres de position de spawn à l'écran
    // -------------------------------------------------------------------------

    // Limite gauche de la zone de spawn horizontal
    [SerializeField] private float _spawnXMin = -2.5f;

    // Limite droite de la zone de spawn horizontal
    [SerializeField] private float _spawnXMax = 2.5f;

    // Hauteur fixe d'apparition des ennemis en haut de l'écran
    [SerializeField] private float _spawnY = 3.5f;

    // -------------------------------------------------------------------------
    // Paramètres des phases de difficulté
    // -------------------------------------------------------------------------

    // Durée en secondes avant l'apparition du VirusHeavy
    [SerializeField] private float _phase2Time = 30f;

    // Durée en secondes avant l'apparition du VirusShooter
    [SerializeField] private float _phase3Time = 60f;

    // -------------------------------------------------------------------------
    // Événement de spawn exposé à SI_VirusInjector
    // -------------------------------------------------------------------------

    // Événement déclenché à chaque spawn de virus pour l'injecteur
    [SerializeField] private UnityEvent<GameObject> _onVirusSpawned;

    /// <summary>Événement public accessible par SI_VirusInjector.</summary>
    // Expose l'événement de spawn de virus en lecture seule
    public UnityEvent<GameObject> OnVirusSpawned => _onVirusSpawned;

    // Son joué lors du spawn d'un virus
    [SerializeField] private AudioClip _spawnClip;

    // Son joué lors d'un changement de phase de difficulté
    [SerializeField] private AudioClip _phaseChangeClip;

    // -------------------------------------------------------------------------
    // État interne du spawn
    // -------------------------------------------------------------------------

    // Temps total écoulé depuis le début de la boucle de spawn
    private float _elapsed;

    // Phase courante de difficulté, utilisée pour détecter les changements
    private int _currentPhase = 1;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Lance la boucle de spawn infinie au démarrage
    private void Start()
    {
        // Vérifie les références aux prefabs de virus avant le spawn
        Debug.Log($"[SI] WaveManager — VirusFast={_virusFastPrefab != null} | VirusHeavy={_virusHeavyPrefab != null} | VirusShooter={_virusShooterPrefab != null}");

        // Démarre la boucle de spawn continu dès le début de la partie
        StartCoroutine(SpawnLoop());
    }

    // -------------------------------------------------------------------------
    // Boucle de spawn continu
    // -------------------------------------------------------------------------

    // Boucle infinie de spawn progressif d'un ennemi à la fois
    private IEnumerator SpawnLoop()
    {
        // Réinitialise le temps écoulé au lancement de la boucle
        _elapsed = 0f;

        // Continue indéfiniment jusqu'à destruction du GameObject
        while (true)
        {
            // Calcule l'intervalle courant décroissant avec le temps
            float interval = Mathf.Max(
                _spawnIntervalMin,
                _spawnIntervalStart - _elapsed * _spawnIntervalDecay
            );

            // WaitForSecondsRealtime ignore Time.timeScale — le spawn continue
            // même si le jeu est mis en pause via Time.timeScale = 0.
            yield return new WaitForSecondsRealtime(interval);

            // Choisit le prefab correspondant à la phase de difficulté
            GameObject prefab = PickPrefab(_elapsed);

            // Vérifie que le prefab sélectionné n'est pas nul avant spawn
            if (prefab == null)
            {
                // Incrémente le temps et continue sans spawner si prefab absent
                _elapsed += interval;
                continue;
            }

            // Génère une position X aléatoire dans la zone de spawn définie
            float x = Random.Range(_spawnXMin, _spawnXMax);

            // Construit la position monde de spawn en haut de l'écran
            Vector3 spawnPos = new Vector3(x, _spawnY, 0f);

            // Instancie l'ennemi à la position calculée sans rotation
            GameObject virusObject = Instantiate(prefab, spawnPos, Quaternion.identity);

            // Vérifie que l'instanciation a réussi avant de notifier
            if (virusObject == null)
            {
                Debug.LogError("[SI_WaveManager] Échec de l'instanciation du virus — spawn ignoré.", this);
                _elapsed += interval;
                continue;
            }

            AudioManager.Instance?.PlaySFX(_spawnClip);

            // Notifie l'injecteur du nouveau virus pour câbler ses références
            _onVirusSpawned?.Invoke(virusObject);

            // Détecte un changement de phase et joue le son associé
            int newPhase = GetCurrentWave();
            if (newPhase != _currentPhase)
            {
                _currentPhase = newPhase;
                AudioManager.Instance?.PlaySFX(_phaseChangeClip);
            }

            // Ajoute l'intervalle écoulé au compteur de temps total
            _elapsed += interval;
        }
    }

    // -------------------------------------------------------------------------
    // Sélection du prefab selon la phase
    // -------------------------------------------------------------------------

    // Sélectionne le prefab ennemi selon le temps de jeu écoulé
    private GameObject PickPrefab(float elapsed)
    {
        // Phase 1 : uniquement VirusFast avant la durée de phase 2
        if (elapsed < _phase2Time)
        {
            // Retourne uniquement le virus rapide en début de partie
            return _virusFastPrefab;
        }

        // Phase 2 : VirusFast ou VirusHeavy à parts égales
        if (elapsed < _phase3Time)
        {
            // Tire au sort entre virus rapide et virus lourd
            return Random.value < 0.5f ? _virusFastPrefab : _virusHeavyPrefab;
        }

        // Phase 3 : les trois types avec poids différenciés
        float roll = Random.value;

        // 50 % de chance de spawner un VirusFast
        if (roll < 0.5f)
        {
            return _virusFastPrefab;
        }

        // 30 % de chance de spawner un VirusHeavy
        if (roll < 0.8f)
        {
            return _virusHeavyPrefab;
        }

        // 20 % de chance de spawner un VirusShooter
        return _virusShooterPrefab;
    }

    // -------------------------------------------------------------------------
    // Accesseurs publics — compatibilité avec les scripts UI existants
    // -------------------------------------------------------------------------

    /// <summary>Retourne la phase de difficulté courante (1, 2 ou 3).</summary>
    // Expose la phase courante comme numéro de vague pour l'affichage UI
    public int GetCurrentWave()
    {
        // Retourne la phase correspondant au temps écoulé depuis le début
        if (_elapsed < _phase2Time) return 1;
        if (_elapsed < _phase3Time) return 2;
        return 3;
    }

    // -------------------------------------------------------------------------
    // Accesseurs diagnostiques — lecture seule, cachés de l'inspecteur
    // -------------------------------------------------------------------------

    /// <summary>Accessor diagnostique — prefab VirusFast.</summary>
    // Expose le prefab VirusFast pour les scripts de diagnostic
    [HideInInspector] public GameObject VirusFastPrefab => _virusFastPrefab;

    /// <summary>Accessor diagnostique — prefab VirusHeavy.</summary>
    // Expose le prefab VirusHeavy pour les scripts de diagnostic
    [HideInInspector] public GameObject VirusHeavyPrefab => _virusHeavyPrefab;

    /// <summary>Accessor diagnostique — prefab VirusShooter.</summary>
    // Expose le prefab VirusShooter pour les scripts de diagnostic
    [HideInInspector] public GameObject VirusShooterPrefab => _virusShooterPrefab;
}
