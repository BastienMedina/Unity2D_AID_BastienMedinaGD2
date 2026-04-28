using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[System.Serializable]
public class InventoryItem
{
    public string Name;
    public string Description;
    public LootEffect Effect;
    public int HealAmount;
    public Sprite Icon;
    public ItemEffectType EffectType;
    public float EffectValue;
    public bool HasDuration;
    public float EffectDuration;
    public ItemData SourceData;
    public int Quantity = 1;
}

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private int _capacity = 9;
    [SerializeField] private LivesManager _livesManager;
    [SerializeField] private PlayerStatsManager _playerStats;
    [SerializeField] private AudioClip _itemAddedClip;
    [SerializeField] private AudioClip _itemConsumedClip;
    [SerializeField] private AudioClip _inventoryFullClip;

    public UnityEvent<int> OnItemAdded                     = new UnityEvent<int>();
    public UnityEvent<int> OnItemConsumed                  = new UnityEvent<int>();
    public UnityEvent OnInventoryFull                      = new UnityEvent();
    public UnityEvent<InventoryItem> OnSpecialItemConsumed = new UnityEvent<InventoryItem>();

    private InventoryItem[] _slots;

    private void Awake() // Alloue les slots et résout LivesManager
    {
        _slots = new InventoryItem[_capacity];
        if (_livesManager == null) _livesManager = FindFirstObjectByType<LivesManager>();
    }

    private void Start() // Restaure l'inventaire depuis GameProgress après tous les Awake
    {
        RestoreFromGameProgress();
    }

    private void OnEnable() // S'abonne aux événements de scène
    {
        SceneManager.sceneLoaded   += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable() // Désabonne pour éviter les appels fantômes
    {
        SceneManager.sceneLoaded   -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) // Réinitialise GameProgress au retour menu
    {
        if (GameProgress.Instance == null) return;
        if (scene.name == "Scene_MainMenu") GameProgress.Instance.Reset(); // Fin de run : retour menu
    }

    private void OnSceneUnloaded(Scene scene) { }

    private void RestoreFromGameProgress() // Copie l'inventaire persisté dans les slots locaux
    {
        if (GameProgress.Instance == null || !GameProgress.Instance.HasPersistedInventory()) return;

        List<InventoryItem> persisted = GameProgress.Instance.PopInventory();

        for (int i = 0; i < persisted.Count && i < _slots.Length; i++) // Remplit les slots dans l'ordre
        {
            _slots[i] = persisted[i];
            if (_slots[i] != null) OnItemAdded.Invoke(i);
        }
    }

    public bool AddItem(InventoryItem item) // Empile sur slot existant ou occupe un slot vide
    {
        if (item == null) return false;

        for (int i = 0; i < _slots.Length; i++) // Cherche un slot empilable du même item
        {
            if (_slots[i] == null) continue;
            bool sameSource = item.SourceData != null && _slots[i].SourceData == item.SourceData;
            bool sameName   = !sameSource && string.Equals(_slots[i].Name, item.Name, System.StringComparison.Ordinal);
            if (!sameSource && !sameName) continue;

            _slots[i].Quantity++;
            OnItemAdded.Invoke(i);
            AudioManager.Instance?.PlaySFX(_itemAddedClip);
            return true;
        }

        for (int i = 0; i < _slots.Length; i++) // Cherche le premier slot vide
        {
            if (_slots[i] != null) continue;
            item.Quantity = 1;
            _slots[i]     = item;
            OnItemAdded.Invoke(i);
            AudioManager.Instance?.PlaySFX(_itemAddedClip);
            return true;
        }

        OnInventoryFull.Invoke(); // Inventaire plein : notifie et joue le son
        AudioManager.Instance?.PlaySFX(_inventoryFullClip);
        return false;
    }

    public InventoryItem GetItem(int slotIndex) // Retourne null si index invalide ou slot vide
    {
        if (!IsValidIndex(slotIndex)) return null;
        return _slots[slotIndex];
    }

    public bool ConsumeItem(int slotIndex) // Applique l'effet, décrémente et libère si vide
    {
        if (!IsValidIndex(slotIndex) || _slots[slotIndex] == null) return false;

        InventoryItem item = _slots[slotIndex];

        if (_playerStats != null) _playerStats.ApplyItemEffect(item); // Priorité PlayerStatsManager
        else ApplyEffect(item);

        item.Quantity--;
        if (item.Quantity <= 0) _slots[slotIndex] = null; // Libère le slot si épuisé

        OnItemConsumed.Invoke(slotIndex);
        AudioManager.Instance?.PlaySFX(_itemConsumedClip);
        return true;
    }

    public int GetCapacity() => _capacity; // Expose la capacité maximale de l'inventaire

    private void ApplyEffect(InventoryItem item) // Applique l'effet étendu ou legacy selon le type
    {
        switch (item.EffectType)
        {
            case ItemEffectType.Heal:             ApplyHeal(item);      return;
            case ItemEffectType.MaxHealthUp:      ApplyMaxHealthUp(item); return;
            case ItemEffectType.NewHeroCartridge: OnSpecialItemConsumed.Invoke(item); return;
            case ItemEffectType.Speed:
            case ItemEffectType.AttackSpeed:
            case ItemEffectType.Damage:
            case ItemEffectType.Shield:           return; // Nécessite PlayerStatsManager
        }

        switch (item.Effect) // Fallback legacy
        {
            case LootEffect.Heal:             ApplyHeal(item);      break;
            case LootEffect.PowerUp:
            case LootEffect.NewHeroCartridge: OnSpecialItemConsumed.Invoke(item); break;
        }
    }

    private void ApplyHeal(InventoryItem item) // Soigne selon EffectValue ou HealAmount
    {
        if (_livesManager == null) return;
        int amount = item.EffectValue > 0f ? (int)item.EffectValue : item.HealAmount;
        _livesManager.Heal(amount);
    }

    private void ApplyMaxHealthUp(InventoryItem item) // Augmente le max et soigne du même montant
    {
        if (_livesManager == null) return;
        int amount = (int)item.EffectValue;
        _livesManager.SetMaxHealth(_livesManager.GetMaxLives() + amount);
        _livesManager.Heal(amount);
    }

    private bool IsValidIndex(int index) => index >= 0 && index < _slots.Length; // Vérifie les bornes du tableau
}
