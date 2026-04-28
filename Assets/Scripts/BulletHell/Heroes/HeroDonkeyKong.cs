using UnityEngine;
using UnityEngine.Events;

public class HeroDonkeyKong : MonoBehaviour
{
    [SerializeField] private GameObject _barrelPrefab;
    [SerializeField] private float _attackCooldown = 0.8f;
    [SerializeField] private float _barrelSpeed = 15f;
    [SerializeField] private int _damage = 1;
    [SerializeField] private UnityEvent _onAttackFired;
    [SerializeField] private AudioClip _attackClip;

    private float _cooldownTimer;

    private void Update() // Décrémente le cooldown d'attaque chaque frame
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;
    }

    public bool CanAttack() => _cooldownTimer <= 0f; // Retourne si l'attaque est disponible

    public void Attack(Vector2 direction) // Lance un baril dans la direction donnée
    {
        if (!CanAttack() || direction == Vector2.zero)
            return;

        Vector2 normalizedDirection = direction.normalized;
        GameObject barrel           = Instantiate(_barrelPrefab, transform.position, Quaternion.identity);
        BarrelProjectile proj       = barrel.GetComponent<BarrelProjectile>();

        if (proj == null) // Valide la présence du composant projectile
        {
            Debug.LogError("[HeroDonkeyKong] BarrelProjectile manquant sur le prefab.", this);
            Destroy(barrel);
            return;
        }

        proj.Initialize(normalizedDirection, _barrelSpeed, _damage);
        _cooldownTimer = _attackCooldown;
        AudioManager.Instance?.PlaySFX(_attackClip);
        _onAttackFired?.Invoke();
    }

    public void SetDamage(int newDamage) // Met à jour les dégâts des projectiles
    {
        _damage = newDamage;
    }

    public void SetAttackCooldown(float newCooldown) // Met à jour le cooldown d'attaque
    {
        _attackCooldown = newCooldown;
    }
}
