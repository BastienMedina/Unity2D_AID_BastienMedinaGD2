using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStatsManager : MonoBehaviour
{
    [SerializeField] private int _baseMaxHealth = 3;
    [SerializeField] private float _baseSpeed = 3f;
    [SerializeField] private int _baseDamage = 1;
    [SerializeField] private float _baseAttackCooldown = 0.8f;
    [SerializeField] private LivesManager _livesManager;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private HeroDonkeyKong _hero;

    private int _currentMaxHealth;
    private float _currentSpeed;
    private int _currentDamage;
    private float _currentAttackCooldown;
    private int _shieldPoints = 0;
    private List<ActiveBuff> _activeBuffs = new List<ActiveBuff>();

    public UnityEvent OnStatsChanged = new UnityEvent();
    public UnityEvent<ActiveBuff> OnBuffAdded = new UnityEvent<ActiveBuff>();
    public UnityEvent<ActiveBuff> OnBuffRemoved = new UnityEvent<ActiveBuff>();

    private void Awake() // Calcule les stats initiales depuis les valeurs de base
    {
        RecalculateStats();
    }

    private void Start() // Restaure les buffs persistés depuis GameProgress
    {
        RestoreFromGameProgress();
    }

    private void Update() // Décrémente les timers de buffs chaque frame
    {
        TickBuffs();
    }

    private void RestoreFromGameProgress() // Restaure buffs et PV max depuis la progression
    {
        if (GameProgress.Instance == null || !GameProgress.Instance.HasPersistedBuffs)
            return;

        int savedBaseMaxHealth = GameProgress.Instance.PopBaseMaxHealth();
        if (savedBaseMaxHealth >= 0) // Met à jour uniquement si valeur persistée valide
        {
            _baseMaxHealth = savedBaseMaxHealth;
            _livesManager?.SetMaxHealth(_baseMaxHealth);
            Debug.Log($"[STATS] PV max restaurés : {_baseMaxHealth}");
        }

        List<ActiveBuff> persistedBuffs = GameProgress.Instance.PopBuffs();
        foreach (ActiveBuff buff in persistedBuffs) // Restaure chaque buff non expiré
        {
            if (buff.HasDuration && buff.RemainingTime <= 0f) continue; // Ignore les buffs expirés
            _activeBuffs.Add(buff);
            OnBuffAdded.Invoke(buff);
        }

        if (_activeBuffs.Count > 0) // Recalcule si des buffs ont été restaurés
            RecalculateStats();
    }

    public void ApplyItemEffect(InventoryItem item) // Applique l'effet d'un objet de l'inventaire
    {
        if (item == null) return;

        switch (item.EffectType) // Redirige vers le handler d'effet
        {
            case ItemEffectType.Heal:             ApplyHeal((int)item.EffectValue);                                                           break;
            case ItemEffectType.MaxHealthUp:      ApplyMaxHealthUp((int)item.EffectValue);                                                    break;
            case ItemEffectType.Speed:            ApplyStatBuff(ItemEffectType.Speed,        item.EffectValue, item.HasDuration, item.EffectDuration, item); break;
            case ItemEffectType.AttackSpeed:      ApplyStatBuff(ItemEffectType.AttackSpeed,  item.EffectValue, item.HasDuration, item.EffectDuration, item); break;
            case ItemEffectType.Damage:           ApplyStatBuff(ItemEffectType.Damage,       item.EffectValue, item.HasDuration, item.EffectDuration, item); break;
            case ItemEffectType.Shield:           ApplyShield((int)item.EffectValue,         item.HasDuration, item.EffectDuration, item);    break;
            case ItemEffectType.NewHeroCartridge: break;
        }

        RecalculateStats();
        OnStatsChanged.Invoke();
    }

    private void ApplyHeal(int amount) // Soigne le joueur du montant donné
    {
        _livesManager.Heal(amount);
    }

    private void ApplyMaxHealthUp(int amount) // Augmente PV max et soigne jusqu'au nouveau max
    {
        _baseMaxHealth += amount;
        _livesManager.SetMaxHealth(_baseMaxHealth);
        _livesManager.Heal(amount);
    }

    private void ApplyStatBuff(ItemEffectType type, float value, bool hasDuration, float duration, InventoryItem sourceItem) // Crée et enregistre un buff de stat
    {
        ActiveBuff buff = new ActiveBuff
        {
            Type           = type,
            Value          = value,
            HasDuration    = hasDuration,
            RemainingTime  = hasDuration ? duration : float.MaxValue,
            TotalDuration  = hasDuration ? duration : float.MaxValue,
            SourceItemName = sourceItem.Name,
            Icon           = sourceItem.Icon
        };

        _activeBuffs.Add(buff);
        OnBuffAdded.Invoke(buff);
    }

    private void ApplyShield(int points, bool hasDuration, float duration, InventoryItem sourceItem) // Ajoute des points de bouclier et crée le buff
    {
        _shieldPoints += points;

        ActiveBuff buff = new ActiveBuff
        {
            Type           = ItemEffectType.Shield,
            Value          = points,
            HasDuration    = hasDuration,
            RemainingTime  = hasDuration ? duration : float.MaxValue,
            TotalDuration  = hasDuration ? duration : float.MaxValue,
            SourceItemName = sourceItem.Name,
            Icon           = sourceItem.Icon
        };

        _activeBuffs.Add(buff);
        OnBuffAdded.Invoke(buff);
    }

    private void RecalculateStats() // Repart de la base et additionne tous les buffs
    {
        _currentSpeed          = _baseSpeed;
        _currentDamage         = _baseDamage;
        _currentAttackCooldown = _baseAttackCooldown;
        _currentMaxHealth      = _baseMaxHealth;

        foreach (ActiveBuff buff in _activeBuffs) // Additionne chaque bonus de buff
        {
            switch (buff.Type)
            {
                case ItemEffectType.Speed:       _currentSpeed         += buff.Value;                                              break;
                case ItemEffectType.Damage:      _currentDamage        += (int)buff.Value;                                        break;
                case ItemEffectType.AttackSpeed: _currentAttackCooldown = Mathf.Max(0.1f, _currentAttackCooldown - buff.Value);   break;
            }
        }

        ApplyStatsToScripts();
    }

    private void ApplyStatsToScripts() // Pousse les stats recalculées vers les composants
    {
        if (_playerMovement != null)
            _playerMovement.SetSpeed(_currentSpeed);

        if (_hero != null)
        {
            _hero.SetDamage(_currentDamage);
            _hero.SetAttackCooldown(_currentAttackCooldown);
        }
    }

    public int AbsorbDamage(int incomingDamage) // Réduit les dégâts via le bouclier
    {
        if (_shieldPoints <= 0) return incomingDamage; // Pas de bouclier actif

        int absorbed  = Mathf.Min(_shieldPoints, incomingDamage);
        _shieldPoints -= absorbed;
        return incomingDamage - absorbed;
    }

    private void TickBuffs() // Décrémente timers et supprime les buffs expirés
    {
        if (_activeBuffs.Count == 0) return;

        List<ActiveBuff> toRemove = new List<ActiveBuff>();

        foreach (ActiveBuff buff in _activeBuffs) // Décrémente chaque buff temporaire
        {
            if (!buff.HasDuration) continue;
            buff.RemainingTime -= Time.deltaTime;
            if (buff.RemainingTime <= 0f) toRemove.Add(buff); // Marque comme expiré
        }

        foreach (ActiveBuff buff in toRemove) // Supprime et notifie chaque buff expiré
        {
            _activeBuffs.Remove(buff);
            OnBuffRemoved.Invoke(buff);
        }

        if (toRemove.Count > 0) // Recalcule seulement si des buffs ont expiré
            RecalculateStats();
    }

    public float GetSpeed()                              => _currentSpeed;
    public int GetDamage()                               => _currentDamage;
    public float GetAttackCooldown()                     => _currentAttackCooldown;
    public int GetMaxHealth()                            => _currentMaxHealth;
    public int GetShieldPoints()                         => _shieldPoints;
    public IReadOnlyList<ActiveBuff> GetActiveBuffs()    => _activeBuffs;
    public int GetBaseMaxHealth()                        => _baseMaxHealth;
}

[System.Serializable]
public class ActiveBuff
{
    public ItemEffectType Type;
    public float Value;
    public bool HasDuration;
    public float RemainingTime;
    public float TotalDuration;
    public string SourceItemName;
    public Sprite Icon;
}
