using UnityEngine;
using UnityEngine.Events;

// Ajoute directement un ItemData à l'inventaire joueur.
public class LootDropper : MonoBehaviour
{
    // Référence au gestionnaire d'inventaire du joueur.
    [SerializeField] private InventoryManager _inventoryManager;

    // Table de loot utilisée pour tirer l'ItemData à attribuer.
    [SerializeField] private LootTable _lootTable;

    // Chance entre 0 et 1 qu'un objet soit effectivement trouvé.
    [SerializeField] private float _dropChance = 0.85f;

    // Son joué lors d'un drop de loot
    [SerializeField] private AudioClip _dropLootClip;

    // Déclenché après chaque drop traité par ce composant.
    public UnityEvent OnLootDropped = new UnityEvent();

    /// <summary>Assigne l'InventoryManager depuis l'extérieur (câblage runtime).</summary>
    public void SetInventoryManager(InventoryManager inventoryManager)
    {
        _inventoryManager = inventoryManager;
    }

    /// <summary>Assigne la LootTable depuis l'extérieur (câblage runtime).</summary>
    public void SetLootTable(LootTable lootTable)
    {
        _lootTable = lootTable;
    }

    // Tente de donner un objet au joueur via l'inventaire.
    public void DropLoot(Vector2 position)
    {
        // Vérifie que la référence à l'inventaire est assignée.
        if (_inventoryManager == null)
        {
            // Signale la référence manquante dans la console.
            Debug.LogWarning("[LootDropper] _inventoryManager non assigné.", this);
            return;
        }

        // Vérifie que la référence à la table de loot est assignée.
        if (_lootTable == null)
        {
            // Signale la table de loot manquante dans la console.
            Debug.LogWarning("[LootDropper] _lootTable non assignée.", this);
            return;
        }

        // Tire le dé pour déterminer si un drop a lieu.
        float roll = Random.Range(0f, 1f);

        // Abandonne si le tirage dépasse la chance configurée.
        if (roll > _dropChance)
            return;

        // Tire un ItemData aléatoire dans la table de loot.
        ItemData item = _lootTable.Roll();

        // Abandonne si la table n'a retourné aucun item valide.
        if (item == null)
            return;

        // Construit l'InventoryItem à partir de l'ItemData tiré.
        InventoryItem invItem = new InventoryItem
        {
            // Nom de l'objet affiché dans l'inventaire joueur.
            Name = item.ItemName,
            // Description affichée dans le panneau de détail.
            Description = item.Description,
            // Type d'effet converti depuis ItemEffectType.
            Effect = LootEffect.PowerUp,
            // Champs étendus portant les données complètes.
            EffectType = item.EffectType,
            EffectValue = item.EffectValue,
            HasDuration = item.HasDuration,
            EffectDuration = item.EffectDuration,
            Icon = item.Icon,
            SourceData = item
        };

        // Tente d'insérer l'item dans le premier slot libre.
        bool added = _inventoryManager.AddItem(invItem);

        // Informe si l'inventaire est plein et l'item non ajouté.
        if (!added)
            Debug.Log("[LootDropper] Inventaire plein — objet non ajouté.");
        else
            Debug.Log($"[LootDropper] Objet ajouté : {item.ItemName} ({item.Rarity})");

        AudioManager.Instance?.PlaySFX(_dropLootClip);

        // Notifie les abonnés que le drop a été traité.
        OnLootDropped.Invoke();
    }
}
