using UnityEngine;
using UnityEngine.Events;

// Détecte la collecte du joueur et applique l'effet du butin
public class LootPickup : MonoBehaviour
{
    // Données de l'item affichées dans l'inventaire (nom, description, icône)
    [SerializeField] private ItemData _itemData;

    // Niveau de rareté de cet objet de butin ramassable
    [SerializeField] private LootRarity _rarity;

    // Effet appliqué au joueur lors de la collecte de cet objet
    [SerializeField] private LootEffect _effect;

    // Nombre de vies restaurées si l'effet est de type Heal
    [SerializeField] private int _healAmount = 1;

    // Référence au gestionnaire de vies pour appliquer les soins
    [SerializeField] private LivesManager _livesManager;

    // Référence au gestionnaire d'inventaire pour les objets collectés
    [SerializeField] private InventoryManager _inventoryManager;

    // Événement déclenché juste avant la destruction de cet objet
    [SerializeField] private UnityEvent _onPickedUp;

    // Détecte le contact avec le joueur et applique l'effet configuré
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore le contact si ce n'est pas le joueur qui touche
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // Applique l'effet correspondant au type configuré sur cet objet
        ApplyEffect();

        // Notifie les abonnés que cet objet vient d'être collecté
        _onPickedUp?.Invoke();

        // Détruit le GameObject après la collecte et l'application
        Destroy(gameObject);
    }

    // Redirige vers la méthode d'effet selon le type configuré
    private void ApplyEffect()
    {
        // Sélectionne l'action à effectuer selon l'effet du butin
        switch (_effect)
        {
            // Applique un soin au joueur via le gestionnaire de vies
            case LootEffect.Heal:
                ApplyHeal();
                break;

            // Ajoute un power-up à l'inventaire du joueur
            case LootEffect.PowerUp:
                ApplyInventoryAdd();
                break;

            // Ajoute une cartouche héros légendaire à l'inventaire
            case LootEffect.NewHeroCartridge:
                ApplyInventoryAdd();
                break;
        }
    }

    // Soigne le joueur du nombre de vies configuré sur cet objet
    private void ApplyHeal()
    {
        // Ignore le soin si le gestionnaire de vies n'est pas assigné
        if (_livesManager == null)
        {
            return;
        }

        // Appelle la méthode de soin avec le montant configuré
        _livesManager.Heal(_healAmount);
    }

    // Construit un InventoryItem et l'ajoute à l'inventaire joueur.
    private void ApplyInventoryAdd()
    {
        // Ignore l'ajout si le gestionnaire d'inventaire n'est pas assigné.
        if (_inventoryManager == null)
        {
            return;
        }

        // Crée un InventoryItem à partir des données de ce pickup.
        InventoryItem item = new InventoryItem
        {
            // Utilise l'ItemData si assigné, sinon le nom du GameObject en fallback.
            Name           = _itemData != null ? _itemData.ItemName    : gameObject.name,
            Description    = _itemData != null ? _itemData.Description : string.Empty,
            Icon           = _itemData != null ? _itemData.Icon        : null,
            Effect         = _effect,
            HealAmount     = _healAmount,
            SourceData     = _itemData,
            // Copie les champs d'effet étendu depuis l'ItemData source
            EffectType     = _itemData != null ? _itemData.EffectType     : ItemEffectType.Heal,
            EffectValue    = _itemData != null ? _itemData.EffectValue    : 0f,
            HasDuration    = _itemData != null && _itemData.HasDuration,
            EffectDuration = _itemData != null ? _itemData.EffectDuration : 0f,
        };

        // Transmet l'InventoryItem au gestionnaire d'inventaire joueur.
        _inventoryManager.AddItem(item);
    }
}
