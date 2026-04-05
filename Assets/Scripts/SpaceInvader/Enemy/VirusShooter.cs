using System.Collections;
using UnityEngine;

// Gère la descente verticale et le tir périodique vers le joueur
public class VirusShooter : VirusBase
{
    // Intervalle en secondes entre deux tirs consécutifs
    [SerializeField] private float _fireInterval = 2f;

    // Prefab de la balle instanciée à chaque tir du virus
    [SerializeField] private GameObject _bulletPrefab;

    // Vitesse transmise à chaque balle instanciée lors du tir
    [SerializeField] private float _bulletSpeed = 5f;

    // Transform du joueur ciblé lors du tir, trouvé automatiquement
    private Transform _playerTransform;

    // Indique si le virus est mort pour bloquer toute action résiduelle
    private bool _isDead;

    // Trouve le joueur et démarre la coroutine de tir si possible
    protected override void Awake()
    {
        // Appelle Awake de VirusBase pour initialiser la santé
        base.Awake();

        // Cherche le GameObject avec le tag Player dans la scène
        GameObject playerGo = GameObject.FindWithTag("Player");

        // Assigne le Transform si le joueur est trouvé dans la scène
        if (playerGo != null)
        {
            // Stocke le Transform pour viser lors de chaque tir
            _playerTransform = playerGo.transform;
        }
        else
        {
            // Avertit si le joueur est absent sans crasher le jeu
            Debug.LogWarning("[VIRUS_SHOOTER] GameObject 'Player' introuvable, tir désactivé");
        }

        // Vérifie la présence du prefab de balle dans l'inspecteur
        if (_bulletPrefab == null)
        {
            // Signale l'absence du prefab sans interrompre l'exécution
            Debug.LogWarning("[VIRUS_SHOOTER] _bulletPrefab non assigné, tir désactivé");
        }
    }

    // Démarre la coroutine de tir après le premier frame
    private void Start()
    {
        // Lance le tir uniquement si les dépendances sont présentes
        if (_playerTransform != null && _bulletPrefab != null)
        {
            // Démarre la boucle de tir périodique vers le joueur
            StartCoroutine(FireRoutine());
        }
    }

    // Tire périodiquement vers la position courante du joueur
    private IEnumerator FireRoutine()
    {
        // Répète tant que le virus est vivant et actif
        while (!_isDead)
        {
            // Attend l'intervalle configuré entre deux tirs
            yield return new WaitForSeconds(_fireInterval);

            // Abandonne si le virus est mort pendant l'attente
            if (_isDead) yield break;

            // Instancie et oriente une balle vers le joueur
            FireAtPlayer();
        }
    }

    // Instancie une balle dirigée vers la position courante du joueur
    private void FireAtPlayer()
    {
        // Abandonne si le prefab ou le joueur est absent
        if (_bulletPrefab == null || _playerTransform == null) return;

        // Calcule la direction normalisée vers le joueur
        Vector2 origin = transform.position;
        Vector2 target = _playerTransform.position;
        Vector2 direction = (target - origin).normalized;

        // Instancie la balle à la position courante du virus
        GameObject bulletGo = Instantiate(_bulletPrefab, origin, Quaternion.identity);

        // Initialise la balle avec la direction et la vitesse calculées
        SI_VirusBullet bullet = bulletGo.GetComponent<SI_VirusBullet>();
        if (bullet != null)
        {
            // Transmet la direction et la vitesse à la balle instanciée
            bullet.Initialize(direction, _bulletSpeed);
        }
    }

    // Arrête le tir et détruit le GameObject à la mort
    protected override void HandleDeath()
    {
        // Empêche un double appel si la mort est déjà traitée
        if (_isDead) return;

        // Signale la mort pour stopper la coroutine de tir
        _isDead = true;

        // Supprime ce virus tireur de la scène après sa mort
        Destroy(gameObject);
    }

    /// <summary>Injecte les références scène (compatibilité avec SI_VirusInjector).</summary>
    // Conservé pour compatibilité avec l'injecteur existant
    public void Inject(WaveManager waveManager, Transform playerTransform)
    {
        // Assigne le Transform joueur si absent et valide
        if (_playerTransform == null && playerTransform != null)
        {
            // Utilise la référence injectée si la recherche auto a échoué
            _playerTransform = playerTransform;
        }
    }
}
