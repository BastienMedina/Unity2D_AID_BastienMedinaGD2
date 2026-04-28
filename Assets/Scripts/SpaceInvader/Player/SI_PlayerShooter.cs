using UnityEngine;
using UnityEngine.Events;

public class SI_PlayerShooter : MonoBehaviour
{
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _fireCooldown = 0.4f;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private UnityEvent _onShotFired;
    [SerializeField] private LivesManager _livesManager;
    [SerializeField] private AudioClip _shootClip;

    private float _cooldownTimer;

    private void Awake() { } // Réservé pour initialisation future

    private void Update() // Décrémente le cooldown de tir chaque frame
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;
    }

    public void Shoot() // Instancie une balle si cooldown et dépendances valides
    {
        if (_livesManager != null && _livesManager.GetCurrentLives() <= 0) return;
        if (_cooldownTimer > 0f) return;

        if (_bulletPrefab == null) { Debug.LogWarning("[SHOOTER] _bulletPrefab manquant, tir ignoré"); return; }
        if (_firePoint    == null) { Debug.LogError("[SHOOTER] _firePoint non assigné");               return; }

        Instantiate(_bulletPrefab, _firePoint.position, Quaternion.identity);
        _cooldownTimer = _fireCooldown; // Réarme le cooldown
        AudioManager.Instance?.PlaySFX(_shootClip);
        _onShotFired?.Invoke();
    }

    public void OnFireButtonDown() => Shoot(); // Déclenché une seule fois par appui
}
