using UnityEngine;
using UnityEngine.Events;

// Gère uniquement le lancer de barils dans une direction donnée
public class HeroDonkeyKong : MonoBehaviour
{
    // Préfab du baril instancié lors de chaque attaque
    [SerializeField] private GameObject _barrelPrefab;

    // Délai minimal entre deux attaques consécutives
    [SerializeField] private float _attackCooldown = 0.8f;

    // Vitesse de déplacement du baril après l'instanciation
    [SerializeField] private float _barrelSpeed = 15f;

    // Dégâts infligés par chaque baril lancé
    [SerializeField] private int _damage = 1;

    // Événement déclenché à chaque attaque réussie
    [SerializeField] private UnityEvent _onAttackFired;

    // Son joué lors d'un lancer de baril
    [SerializeField] private AudioClip _attackClip;

    // Temps restant avant que la prochaine attaque soit permise
    private float _cooldownTimer = 0f;

    // Décrémente le cooldown à chaque frame écoulée
    private void Update()
    {
        // Réduit le timer uniquement s'il reste du temps à attendre
        if (_cooldownTimer > 0f)
        {
            _cooldownTimer -= Time.deltaTime;
        }
    }

    // Retourne vrai si le cooldown est expiré et l'attaque possible
    public bool CanAttack()
    {
        // Vérifie que le timer est épuisé avant d'autoriser l'attaque
        return _cooldownTimer <= 0f;
    }

    // Lance un baril dans la direction fournie si possible
    public void Attack(Vector2 direction)
    {
        // Bloque l'attaque si le cooldown n'est pas encore expiré
        if (!CanAttack())
        {
            return;
        }

        // Ignore l'attaque si aucune direction valide n'est fournie
        if (direction == Vector2.zero)
        {
            return;
        }

        // Normalise la direction pour éviter un lancer en biais
        Vector2 normalizedDirection = direction.normalized;

        // Instancie le baril à la position actuelle du joueur
        GameObject barrel = Instantiate(_barrelPrefab, transform.position, Quaternion.identity);

        // Récupère le script du baril pour initialiser ses paramètres
        BarrelProjectile barrelProjectile = barrel.GetComponent<BarrelProjectile>();

        // Vérifie que le composant est présent avant d'appeler Initialize
        if (barrelProjectile == null)
        {
            Debug.LogError("[HeroDonkeyKong] Le prefab baril n'a pas de composant BarrelProjectile.", this);
            Destroy(barrel);
            return;
        }

        // Initialise le baril avec direction, vitesse et dégâts configurés
        barrelProjectile.Initialize(normalizedDirection, _barrelSpeed, _damage);

        // Réarme le cooldown après un lancer réussi
        _cooldownTimer = _attackCooldown;

        AudioManager.Instance?.PlaySFX(_attackClip);

        // Notifie les abonnés qu'une attaque vient d'être lancée
        _onAttackFired?.Invoke();
    }

    // Modifie les dégâts des projectiles depuis l'extérieur
    public void SetDamage(int newDamage)
    {
        // Met à jour les dégâts configurés
        _damage = newDamage;
    }

    // Modifie le cooldown d'attaque depuis l'extérieur
    public void SetAttackCooldown(float newCooldown)
    {
        // Met à jour le cooldown avec la nouvelle valeur
        _attackCooldown = newCooldown;
    }
}
