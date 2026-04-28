using UnityEngine;
using UnityEngine.Events;

public class LootDropper : MonoBehaviour
{
    [SerializeField] private InventoryManager _inventoryManager;
    [SerializeField] private LootTable _lootTable;
    [SerializeField] private float _dropChance = 0.85f;
    [SerializeField] private AudioClip _dropLootClip;

    public UnityEvent OnLootDropped = new UnityEvent();

    public void SetInventoryManager(InventoryManager inventoryManager) // Assigne l'inventaire depuis l'extérieur
    {
        _inventoryManager = inventoryManager;
    }

    public void SetLootTable(LootTable lootTable) // Assigne la table de loot depuis l'extérieur
    {
        _lootTable = lootTable;
    }

    public void DropLoot(Vector2 position) // Tire un objet et l'ajoute à l'inventaire
    {
        if (_inventoryManager == null)
        {
            Debug.LogWarning("[LootDropper] _inventoryManager non assigné.", this);
            return;
        }

        if (_lootTable == null)
        {
            Debug.LogWarning("[LootDropper] _lootTable non assignée.", this);
            return;
        }

        if (Random.Range(0f, 1f) > _dropChance) // Abandonne si le tirage rate
            return;

        ItemData item = _lootTable.Roll();
        if (item == null)
            return;

        InventoryItem invItem = new InventoryItem
        {
            Name           = item.ItemName,
            Description    = item.Description,
            Effect         = LootEffect.PowerUp,
            EffectType     = item.EffectType,
            EffectValue    = item.EffectValue,
            HasDuration    = item.HasDuration,
            EffectDuration = item.EffectDuration,
            Icon           = item.Icon,
            SourceData     = item
        };

        bool added = _inventoryManager.AddItem(invItem);
        if (!added)
            Debug.LogWarning("[LootDropper] Inventaire plein — objet non ajouté.");

        AudioManager.Instance?.PlaySFX(_dropLootClip);
        OnLootDropped.Invoke();
    }
}
