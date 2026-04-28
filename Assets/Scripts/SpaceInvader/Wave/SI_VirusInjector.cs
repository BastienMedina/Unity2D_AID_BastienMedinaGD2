using UnityEngine;

[RequireComponent(typeof(SI_WaveManager))]
public class SI_VirusInjector : MonoBehaviour
{
    [SerializeField] private WaveManager _waveManager;
    [SerializeField] private SI_ServerHealth _serverHealth;
    [SerializeField] private Transform _playerTransform;

    private SI_WaveManager _siWaveManager;

    private void Awake() // Récupère le WaveManager et résout les références scène
    {
        _siWaveManager = GetComponent<SI_WaveManager>();

        if (_waveManager == null)
        {
            _waveManager = FindObjectOfType<WaveManager>();
            if (_waveManager == null) Debug.LogError("[SI] SI_VirusInjector — WaveManager introuvable.");
        }

        if (_serverHealth == null)
            _serverHealth = FindObjectOfType<SI_ServerHealth>();

        if (_playerTransform == null)
        {
            GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo != null) _playerTransform = playerGo.transform;
            else Debug.LogWarning("[SI] SI_VirusInjector — GameObject 'Player' introuvable.");
        }
    }

    private void OnEnable()  // S'abonne à l'événement de spawn de virus
    {
        if (_siWaveManager != null) _siWaveManager.OnVirusSpawned.AddListener(InjectVirus);
    }

    private void OnDisable() // Se désabonne pour éviter les fuites mémoire
    {
        if (_siWaveManager != null) _siWaveManager.OnVirusSpawned.RemoveListener(InjectVirus);
    }

    private void InjectVirus(GameObject virusGo) // Injecte les refs scène dans le virus spawné
    {
        if (virusGo == null) return;

        VirusFast fast = virusGo.GetComponent<VirusFast>();
        if (fast != null)
        {
            fast.Inject(_waveManager, _serverHealth);
            _waveManager?.RegisterBasePosition(fast, virusGo.transform.position.x); // Enregistre la X de base
        }

        VirusHeavy heavy = virusGo.GetComponent<VirusHeavy>();
        if (heavy != null)
        {
            heavy.Inject(_waveManager);
            _waveManager?.RegisterBasePosition(heavy, virusGo.transform.position.x);
        }

        VirusShooter shooter = virusGo.GetComponent<VirusShooter>();
        if (shooter != null)
        {
            shooter.Inject(_waveManager, _playerTransform);
            _waveManager?.RegisterBasePosition(shooter, virusGo.transform.position.x);
        }

        VirusMini mini = virusGo.GetComponent<VirusMini>();
        if (mini != null) mini.Inject(_serverHealth); // Injecte la santé serveur au mini-virus
    }
}
