using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Gère toutes les statistiques du joueur
public class PlayerStatsManager : MonoBehaviour
{
    // ── STATS DE BASE ────────────────────────────────

    // Vie maximale de base du joueur
    [SerializeField] private int _baseMaxHealth = 3;

    // Vitesse de déplacement de base
    [SerializeField] private float _baseSpeed = 3f;

    // Dégâts de base des projectiles
    [SerializeField] private int _baseDamage = 1;

    // Vitesse d'attaque de base en secondes
    [SerializeField] private float _baseAttackCooldown = 0.8f;

    // ── STATS COURANTES (base + bonus) ───────────────

    // Vie maximale actuelle avec bonus appliqués
    private int _currentMaxHealth;

    // Vitesse courante avec bonus appliqués
    private float _currentSpeed;

    // Dégâts courants avec bonus appliqués
    private int _currentDamage;

    // Cooldown d'attaque courant avec bonus appliqués
    private float _currentAttackCooldown;

    // ── BOUCLIER ─────────────────────────────────────

    // Points de bouclier absorbant les dégâts
    private int _shieldPoints = 0;

    // ── BUFFS ACTIFS ─────────────────────────────────

    // Liste des buffs temporaires actifs
    private List<ActiveBuff> _activeBuffs = new List<ActiveBuff>();

    // ── RÉFÉRENCES ───────────────────────────────────

    // Référence au gestionnaire de vies
    [SerializeField] private LivesManager _livesManager;

    // Référence au script de déplacement
    [SerializeField] private PlayerMovement _playerMovement;

    // Référence au héros pour les dégâts
    [SerializeField] private HeroDonkeyKong _hero;

    // ── ÉVÉNEMENTS ───────────────────────────────────

    // Déclenché quand les stats changent
    public UnityEvent OnStatsChanged = new UnityEvent();

    // Déclenché quand un buff est ajouté
    public UnityEvent<ActiveBuff> OnBuffAdded = new UnityEvent<ActiveBuff>();

    // Déclenché quand un buff expire
    public UnityEvent<ActiveBuff> OnBuffRemoved = new UnityEvent<ActiveBuff>();

    // Initialise les stats au démarrage
    private void Awake()
    {
        // Calcule les stats initiales depuis les valeurs de base
        RecalculateStats();
    }

    // Restaure les buffs persistés depuis GameProgress après tous les Awake()
    private void Start()
    {
        RestoreFromGameProgress();
    }

    // Met à jour les buffs temporaires chaque frame
    private void Update()
    {
        // Décrémente les timers de tous les buffs actifs
        TickBuffs();
    }

    // ── RESTAURATION ─────────────────────────────────

    // Restaure les buffs et les PV max de base depuis GameProgress
    private void RestoreFromGameProgress()
    {
        if (GameProgress.Instance == null || !GameProgress.Instance.HasPersistedBuffs)
            return;

        // Restaure les PV max de base si persistés
        int savedBaseMaxHealth = GameProgress.Instance.PopBaseMaxHealth();
        if (savedBaseMaxHealth >= 0)
        {
            _baseMaxHealth = savedBaseMaxHealth;
            _livesManager?.SetMaxHealth(_baseMaxHealth);
            Debug.Log($"[STATS] PV max restaurés : {_baseMaxHealth}");
        }

        // Restaure les buffs actifs — les buffs temporaires reprennent avec leur temps restant
        List<ActiveBuff> persistedBuffs = GameProgress.Instance.PopBuffs();
        foreach (ActiveBuff buff in persistedBuffs)
        {
            // Ignore les buffs temporaires expirés pendant la transition
            if (buff.HasDuration && buff.RemainingTime <= 0f)
                continue;

            _activeBuffs.Add(buff);
            OnBuffAdded.Invoke(buff);
            Debug.Log($"[STATS] Buff restauré : {buff.Type} +{buff.Value}");
        }

        // Recalcule les stats avec les buffs restaurés
        if (_activeBuffs.Count > 0)
            RecalculateStats();
    }

    // ── APPLICATION DES EFFETS ───────────────────────

    // Applique l'effet d'un objet consommé
    public void ApplyItemEffect(InventoryItem item)
    {
        // Vérifie que l'objet est valide
        if (item == null) return;

        // Sélectionne l'effet selon le type de l'objet
        switch (item.EffectType)
        {
            case ItemEffectType.Heal:
                // Applique un soin instantané au joueur
                ApplyHeal((int)item.EffectValue);
                break;

            case ItemEffectType.MaxHealthUp:
                // Augmente les PV max et soigne le joueur
                ApplyMaxHealthUp((int)item.EffectValue);
                break;

            case ItemEffectType.Speed:
                // Applique un bonus de vitesse
                ApplyStatBuff(ItemEffectType.Speed,
                    item.EffectValue, item.HasDuration, item.EffectDuration, item);
                break;

            case ItemEffectType.AttackSpeed:
                // Applique un bonus de vitesse d'attaque
                ApplyStatBuff(ItemEffectType.AttackSpeed,
                    item.EffectValue, item.HasDuration, item.EffectDuration, item);
                break;

            case ItemEffectType.Damage:
                // Applique un bonus de dégâts
                ApplyStatBuff(ItemEffectType.Damage,
                    item.EffectValue, item.HasDuration, item.EffectDuration, item);
                break;

            case ItemEffectType.Shield:
                // Applique un bouclier absorbant les dégâts
                ApplyShield((int)item.EffectValue,
                    item.HasDuration, item.EffectDuration, item);
                break;

            case ItemEffectType.NewHeroCartridge:
                // Enregistre la cartouche pour la prochaine run
                Debug.Log($"[STATS] Cartouche héros obtenue : {item.Name}");
                break;
        }

        // Recalcule toutes les stats après application
        RecalculateStats();

        // Notifie les abonnés du changement de stats
        OnStatsChanged.Invoke();
    }

    // ── EFFETS INDIVIDUELS ───────────────────────────

    // Soigne le joueur d'un montant donné
    private void ApplyHeal(int amount)
    {
        // Appelle le soin sur le LivesManager
        _livesManager.Heal(amount);

        // Confirme le soin appliqué
        Debug.Log($"[STATS] Soin appliqué : +{amount} PV");
    }

    // Augmente les PV max et soigne jusqu'au nouveau max
    private void ApplyMaxHealthUp(int amount)
    {
        // Incrémente le max de vie de base
        _baseMaxHealth += amount;

        // Met à jour le max dans LivesManager
        _livesManager.SetMaxHealth(_baseMaxHealth);

        // Soigne le joueur jusqu'au nouveau maximum
        _livesManager.Heal(amount);

        // Confirme l'augmentation des PV max
        Debug.Log($"[STATS] PV max augmentés : +{amount} → {_baseMaxHealth}");
    }

    // Applique un buff de stat temporaire ou permanent
    private void ApplyStatBuff(ItemEffectType type, float value,
        bool hasDuration, float duration,
        InventoryItem sourceItem)
    {
        // Crée le buff avec toutes ses propriétés
        ActiveBuff buff = new ActiveBuff
        {
            Type          = type,
            Value         = value,
            HasDuration   = hasDuration,
            RemainingTime = hasDuration ? duration : float.MaxValue,
            TotalDuration = hasDuration ? duration : float.MaxValue,
            SourceItemName = sourceItem.Name,
            Icon          = sourceItem.Icon
        };

        // Ajoute le buff à la liste active
        _activeBuffs.Add(buff);

        // Notifie l'UI qu'un nouveau buff est actif
        OnBuffAdded.Invoke(buff);

        // Confirme le buff dans la console
        Debug.Log($"[STATS] Buff ajouté : {type} +{value}" +
            (hasDuration ? $" ({duration}s)" : " (permanent)"));
    }

    // Applique un bouclier absorbant les prochains dégâts
    private void ApplyShield(int points, bool hasDuration,
        float duration, InventoryItem sourceItem)
    {
        // Ajoute les points de bouclier
        _shieldPoints += points;

        // Crée le buff bouclier pour l'affichage
        ActiveBuff buff = new ActiveBuff
        {
            Type          = ItemEffectType.Shield,
            Value         = points,
            HasDuration   = hasDuration,
            RemainingTime = hasDuration ? duration : float.MaxValue,
            TotalDuration = hasDuration ? duration : float.MaxValue,
            SourceItemName = sourceItem.Name,
            Icon          = sourceItem.Icon
        };

        // Ajoute et notifie comme les autres buffs
        _activeBuffs.Add(buff);
        OnBuffAdded.Invoke(buff);

        // Confirme le bouclier dans la console
        Debug.Log($"[STATS] Bouclier appliqué : +{points} points");
    }

    // ── RECALCUL DES STATS ───────────────────────────

    // Recalcule toutes les stats depuis la base + buffs
    private void RecalculateStats()
    {
        // Repart des valeurs de base à chaque recalcul
        _currentSpeed          = _baseSpeed;
        _currentDamage         = _baseDamage;
        _currentAttackCooldown = _baseAttackCooldown;
        _currentMaxHealth      = _baseMaxHealth;

        // Additionne les bonus de tous les buffs actifs
        foreach (ActiveBuff buff in _activeBuffs)
        {
            // Applique le bonus selon le type du buff
            switch (buff.Type)
            {
                case ItemEffectType.Speed:
                    // Additionne le bonus de vitesse
                    _currentSpeed += buff.Value;
                    break;

                case ItemEffectType.Damage:
                    // Additionne le bonus de dégâts
                    _currentDamage += (int)buff.Value;
                    break;

                case ItemEffectType.AttackSpeed:
                    // Réduit le cooldown d'attaque
                    _currentAttackCooldown =
                        Mathf.Max(0.1f, _currentAttackCooldown - buff.Value);
                    break;
            }
        }

        // Applique les stats recalculées aux scripts concernés
        ApplyStatsToScripts();
    }

    // Pousse les stats recalculées vers les scripts joueur
    private void ApplyStatsToScripts()
    {
        // Met à jour la vitesse dans PlayerMovement
        if (_playerMovement != null)
            _playerMovement.SetSpeed(_currentSpeed);

        // Met à jour dégâts et cooldown dans HeroDonkeyKong
        if (_hero != null)
        {
            _hero.SetDamage(_currentDamage);
            _hero.SetAttackCooldown(_currentAttackCooldown);
        }
    }

    // ── BOUCLIER ─────────────────────────────────────

    // Intercepte les dégâts et les réduit via le bouclier
    public int AbsorbDamage(int incomingDamage)
    {
        // Retourne les dégâts intacts si pas de bouclier
        if (_shieldPoints <= 0) return incomingDamage;

        // Calcule les dégâts absorbés par le bouclier
        int absorbed = Mathf.Min(_shieldPoints, incomingDamage);
        _shieldPoints -= absorbed;

        // Log l'absorption du bouclier
        Debug.Log($"[STATS] Bouclier absorbe {absorbed} dégât(s) — reste {_shieldPoints}");

        // Retourne les dégâts restants après absorption
        return incomingDamage - absorbed;
    }

    // ── BUFFS TEMPORAIRES ────────────────────────────

    // Décrémente les timers et expire les buffs terminés
    private void TickBuffs()
    {
        // Ignore s'il n'y a aucun buff actif
        if (_activeBuffs.Count == 0) return;

        // Liste des buffs à supprimer ce frame
        List<ActiveBuff> toRemove = new List<ActiveBuff>();

        // Décrémente chaque buff temporaire
        foreach (ActiveBuff buff in _activeBuffs)
        {
            // Ignore les buffs permanents
            if (!buff.HasDuration) continue;

            // Décrémente le temps restant
            buff.RemainingTime -= Time.deltaTime;

            // Marque le buff comme expiré si le timer est écoulé
            if (buff.RemainingTime <= 0f)
                toRemove.Add(buff);
        }

        // Supprime et notifie chaque buff expiré
        foreach (ActiveBuff buff in toRemove)
        {
            _activeBuffs.Remove(buff);
            OnBuffRemoved.Invoke(buff);

            // Confirme l'expiration du buff
            Debug.Log($"[STATS] Buff expiré : {buff.Type}");
        }

        // Recalcule les stats si des buffs ont expiré
        if (toRemove.Count > 0)
            RecalculateStats();
    }

    // ── ACCESSEURS PUBLICS ───────────────────────────

    // Retourne la vitesse courante du joueur
    public float GetSpeed() => _currentSpeed;

    // Retourne les dégâts courants du joueur
    public int GetDamage() => _currentDamage;

    // Retourne le cooldown d'attaque courant
    public float GetAttackCooldown() => _currentAttackCooldown;

    // Retourne les PV max courants
    public int GetMaxHealth() => _currentMaxHealth;

    // Retourne les points de bouclier restants
    public int GetShieldPoints() => _shieldPoints;

    // Retourne la liste des buffs actifs en lecture seule
    public IReadOnlyList<ActiveBuff> GetActiveBuffs() => _activeBuffs;

    // Retourne la valeur de base des PV max (avant buffs)
    public int GetBaseMaxHealth() => _baseMaxHealth;
}

// Représente un buff temporaire ou permanent actif
[System.Serializable]
public class ActiveBuff
{
    // Type de stat affectée par ce buff
    public ItemEffectType Type;

    // Valeur du bonus appliqué
    public float Value;

    // Indique si le buff a une durée limitée
    public bool HasDuration;

    // Temps restant avant expiration du buff
    public float RemainingTime;

    // Durée totale du buff pour le calcul UI
    public float TotalDuration;

    // Nom de l'objet source pour affichage
    public string SourceItemName;

    // Icône de l'objet source pour l'UI
    public Sprite Icon;
}
