using System.Collections;
using UnityEngine;

public class VirusShooter : VirusBase
{
    [SerializeField] private float _fireInterval = 2f;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _bulletSpeed = 5f;
    [SerializeField] private AudioClip _shootClip;

    private Transform _playerTransform;
    private bool _isDead;

    protected override void Awake() // Localise le joueur et valide le prefab de balle
    {
        base.Awake();

        GameObject playerGo = GameObject.FindWithTag("Player");
        if (playerGo != null)
            _playerTransform = playerGo.transform;
        else
            Debug.LogWarning("[VIRUS_SHOOTER] GameObject 'Player' introuvable, tir désactivé");

        if (_bulletPrefab == null)
            Debug.LogWarning("[VIRUS_SHOOTER] _bulletPrefab non assigné, tir désactivé");
    }

    private void Start() // Démarre la coroutine de tir si les dépendances sont prêtes
    {
        if (_playerTransform != null && _bulletPrefab != null)
            StartCoroutine(FireRoutine());
    }

    private IEnumerator FireRoutine() // Tire périodiquement vers le joueur
    {
        while (!_isDead)
        {
            yield return new WaitForSeconds(_fireInterval); // Attend l'intervalle configuré
            if (_isDead) yield break;
            FireAtPlayer();
        }
    }

    private void FireAtPlayer() // Instancie une balle dirigée vers le joueur
    {
        if (_bulletPrefab == null || _playerTransform == null) return;

        Vector2 direction = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;
        GameObject bulletGo = Instantiate(_bulletPrefab, transform.position, Quaternion.identity);
        AudioManager.Instance?.PlaySFX(_shootClip);

        SI_VirusBullet bullet = bulletGo.GetComponent<SI_VirusBullet>();
        if (bullet != null) bullet.Initialize(direction, _bulletSpeed); // Injecte direction et vitesse
    }

    protected override void HandleDeath() // Stoppe le tir et détruit le virus
    {
        if (_isDead) return;
        _isDead = true;
        Destroy(gameObject);
    }

    public void Inject(WaveManager waveManager, Transform playerTransform) // Injecte le joueur si la recherche auto a échoué
    {
        if (_playerTransform == null && playerTransform != null)
            _playerTransform = playerTransform;
    }
}
