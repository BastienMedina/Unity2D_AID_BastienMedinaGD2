using UnityEngine;
using UnityEngine.Events;

// Gère uniquement la cadence de tir et l'instanciation des balles
public class SI_PlayerShooter : MonoBehaviour
{
    // Prefab de la balle instanciée à chaque tir valide
    [SerializeField] private GameObject _bulletPrefab;

    // Délai minimum entre deux tirs consécutifs en secondes
    [SerializeField] private float _fireCooldown = 0.4f;

    // Point de spawn de la balle au-dessus du joueur
    [SerializeField] private Transform _firePoint;

    // Événement déclenché à chaque tir validé avec succès
    [SerializeField] private UnityEvent _onShotFired;

    // Référence au gestionnaire de vies pour vérifier si le joueur est mort
    [SerializeField] private LivesManager _livesManager;

    // Temps restant avant que le prochain tir soit autorisé
    private float _cooldownTimer;

    // Vérifie les références critiques au démarrage
    private void Awake()
    {
        // Vérifie la référence au prefab de balle
        Debug.Log($"[SI] SI_PlayerShooter — _bulletPrefab={_bulletPrefab != null} | _firePoint={_firePoint != null}");
    }

    // Décrémente le timer de cooldown chaque frame
    private void Update()
    {
        // Réduit le timer seulement s'il reste du temps à attendre
        if (_cooldownTimer > 0f)
        {
            // Soustrait le temps écoulé depuis la dernière frame
            _cooldownTimer -= Time.deltaTime;
        }
    }

    /// <summary>Appelée par le bouton UI pour tenter un tir.</summary>
    // Vérifie le cooldown, instancie la balle et notifie les abonnés
    public void Shoot()
    {
        // Confirme que Shoot() est bien appelé
        Debug.Log($"[SI] Shoot() appelé — cooldownReady={_cooldownTimer <= 0f} | timer={_cooldownTimer}");

        // Bloque le tir si le joueur n'a plus de vies
        if (_livesManager != null && _livesManager.GetCurrentLives() <= 0)
        {
            return;
        }

        // Bloque le tir si le cooldown n'est pas encore écoulé
        if (_cooldownTimer > 0f)
        {
            // Signale que le cooldown bloque le tir
            Debug.Log($"[SI] Shoot() BLOQUÉ — timer={_cooldownTimer}");
            return;
        }

        // Avertit si le prefab de balle est absent et stoppe le tir
        if (_bulletPrefab == null)
        {
            Debug.LogWarning("[SHOOTER] SI_PlayerShooter — _bulletPrefab manquant, tir ignoré");
            return;
        }

        // Vérifie que le point de tir est assigné dans l'inspecteur
        if (_firePoint == null)
        {
            Debug.LogError("[SHOOTER] SI_PlayerShooter — _firePoint non assigné dans l'inspecteur");
            return;
        }

        // Confirme l'instanciation de la balle
        Debug.Log($"[SI] Shoot() — instanciation balle à {_firePoint.position}");

        // Instancie la balle à la position et rotation du point de tir
        Instantiate(_bulletPrefab, _firePoint.position, Quaternion.identity);

        // Réarme le timer au cooldown configuré après un tir réussi
        _cooldownTimer = _fireCooldown;

        // Notifie les abonnés qu'un tir vient d'être effectué
        _onShotFired?.Invoke();
    }

    /// <summary>Point d'entrée EventTrigger PointerDown — un seul appel par appui.</summary>
    // Déclenché une seule fois par appui sur le bouton
    public void OnFireButtonDown()
    {
        // Appelle Shoot une seule fois par appui
        Shoot();
    }
}
