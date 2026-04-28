using UnityEngine;
using UnityEngine.Events;

public class LootPickup : MonoBehaviour
{
    [SerializeField] private ItemData _itemData;
    [SerializeField] private LootRarity _rarity;
    [SerializeField] private LootEffect _effect;
    [SerializeField] private int _healAmount = 1;
    [SerializeField] private LivesManager _livesManager;
    [SerializeField] private InventoryManager _inventoryManager;
    [SerializeField] private UnityEvent _onPickedUp;

    private void OnTriggerEnter2D(Collider2D other) // Applique l'effet et se détruit si joueur
    {
        if (!other.CompareTag("Player"))
            return;

        ApplyEffect();
        _onPickedUp?.Invoke();
        Destroy(gameObject);
    }

    private void ApplyEffect() // Redirige vers le handler d'effet configuré
    {
        switch (_effect) // Sélectionne l'action selon le type
        {
            case LootEffect.Heal:             ApplyHeal();         break;
            case LootEffect.PowerUp:          ApplyInventoryAdd(); break;
            case LootEffect.NewHeroCartridge: ApplyInventoryAdd(); break;
        }
    }

    private void ApplyHeal() // Soigne le joueur du montant configuré
    {
        if (_livesManager == null) return;
        _livesManager.Heal(_healAmount);
    }

    private void ApplyInventoryAdd() // Construit et ajoute l'item à l'inventaire
    {
        if (_inventoryManager == null) return;

        InventoryItem item = new InventoryItem
        {
            Name           = _itemData != null ? _itemData.ItemName    : gameObject.name,
            Description    = _itemData != null ? _itemData.Description : string.Empty,
            Icon           = _itemData != null ? _itemData.Icon        : null,
            Effect         = _effect,
            HealAmount     = _healAmount,
            SourceData     = _itemData,
            EffectType     = _itemData != null ? _itemData.EffectType     : ItemEffectType.Heal,
            EffectValue    = _itemData != null ? _itemData.EffectValue    : 0f,
            HasDuration    = _itemData != null && _itemData.HasDuration,
            EffectDuration = _itemData != null ? _itemData.EffectDuration : 0f,
        };

        _inventoryManager.AddItem(item);
    }
}
