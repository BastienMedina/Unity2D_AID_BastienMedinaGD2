using UnityEngine;
using UnityEngine.Events;

// Injecte les références scène dans chaque virus spawné au runtime
[RequireComponent(typeof(SI_WaveManager))]
public class SI_VirusInjector : MonoBehaviour
{
    // Référence au WaveManager utilisé par les scripts virus existants
    [SerializeField] private WaveManager _waveManager;

    // Référence à la santé du serveur transmise aux virus plongeurs
    [SerializeField] private SI_ServerHealth _serverHealth;

    // Transform du joueur transmis au VirusShooter pour viser
    [SerializeField] private Transform _playerTransform;

    // Référence au gestionnaire de spawn pour s'abonner à ses events
    private SI_WaveManager _siWaveManager;

    // Récupère le SI_WaveManager et s'abonne à l'événement de spawn
    private void Awake()
    {
        // Récupère SI_WaveManager sur le même GameObject
        _siWaveManager = GetComponent<SI_WaveManager>();

        // Vérifie la présence du WaveManager dans l'inspecteur
        if (_waveManager == null)
        {
            // Tente de le trouver automatiquement dans la scène
            _waveManager = FindObjectOfType<WaveManager>();

            // Signale l'absence si la recherche automatique échoue aussi
            if (_waveManager == null)
            {
                Debug.LogError("[SI] SI_VirusInjector — WaveManager introuvable dans la scène.");
            }
        }

        // Tente de trouver SI_ServerHealth automatiquement si absent
        if (_serverHealth == null)
        {
            // Cherche SI_ServerHealth dans toute la scène active
            _serverHealth = FindObjectOfType<SI_ServerHealth>();
        }

        // Tente de trouver le Transform joueur automatiquement si absent
        if (_playerTransform == null)
        {
            // Cherche le GameObject taggué Player dans la scène
            GameObject playerGo = GameObject.FindGameObjectWithTag("Player");

            // Assigne le Transform si le joueur est trouvé
            if (playerGo != null)
            {
                _playerTransform = playerGo.transform;
            }
            else
            {
                // Signale l'absence du joueur si la recherche échoue
                Debug.LogWarning("[SI] SI_VirusInjector — GameObject 'Player' introuvable.");
            }
        }
    }

    // S'abonne à l'événement OnVirusSpawned au démarrage
    private void OnEnable()
    {
        // Ignore si le SI_WaveManager est absent du GameObject
        if (_siWaveManager == null) return;

        // Abonne l'injection à chaque nouveau virus spawné
        _siWaveManager.OnVirusSpawned.AddListener(InjectVirus);
    }

    // Se désabonne pour éviter les fuites mémoire à la désactivation
    private void OnDisable()
    {
        // Ignore si le SI_WaveManager est absent du GameObject
        if (_siWaveManager == null) return;

        // Retire l'abonnement à l'événement de spawn de virus
        _siWaveManager.OnVirusSpawned.RemoveListener(InjectVirus);
    }

    // Injecte toutes les références scène dans le virus reçu
    private void InjectVirus(GameObject virusGo)
    {
        // Ignore si le GameObject de virus reçu est null
        if (virusGo == null) return;

        // Injecte WaveManager dans VirusFast si présent sur ce GO
        VirusFast fast = virusGo.GetComponent<VirusFast>();
        if (fast != null)
        {
            // Assigne le WaveManager et le ServerHealth au VirusFast
            fast.Inject(_waveManager, _serverHealth);

            // Enregistre la position X de base dans le WaveManager
            if (_waveManager != null)
            {
                _waveManager.RegisterBasePosition(fast, virusGo.transform.position.x);
            }
        }

        // Injecte WaveManager dans VirusHeavy si présent sur ce GO
        VirusHeavy heavy = virusGo.GetComponent<VirusHeavy>();
        if (heavy != null)
        {
            // Assigne uniquement le WaveManager au VirusHeavy
            heavy.Inject(_waveManager);

            // Enregistre la position X de base dans le WaveManager
            if (_waveManager != null)
            {
                _waveManager.RegisterBasePosition(heavy, virusGo.transform.position.x);
            }
        }

        // Injecte WaveManager et joueur dans VirusShooter si présent
        VirusShooter shooter = virusGo.GetComponent<VirusShooter>();
        if (shooter != null)
        {
            // Assigne WaveManager et Transform joueur au VirusShooter
            shooter.Inject(_waveManager, _playerTransform);

            // Enregistre la position X de base dans le WaveManager
            if (_waveManager != null)
            {
                _waveManager.RegisterBasePosition(shooter, virusGo.transform.position.x);
            }
        }

        // Injecte le ServerHealth dans VirusMini si présent sur ce GO
        VirusMini mini = virusGo.GetComponent<VirusMini>();
        if (mini != null)
        {
            // Assigne le ServerHealth au VirusMini pour l'impact
            mini.Inject(_serverHealth);
        }
    }
}
