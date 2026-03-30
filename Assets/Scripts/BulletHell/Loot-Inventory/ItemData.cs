using UnityEngine;

// Type d'effet que l'objet applique au joueur.
public enum ItemEffectType
{
    // Soin instantané des points de vie.
    Heal,
    // Augmente les points de vie maximum.
    MaxHealthUp,
    // Augmente les dégâts infligés.
    Damage,
    // Augmente la vitesse de déplacement.
    Speed,
    // Augmente la vitesse d'attaque.
    AttackSpeed,
    // Absorbe des dégâts temporairement.
    Shield,
    // Débloque un nouveau héros jouable.
    NewHeroCartridge
}

// Stocke les données d'un objet de butin.
[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    // Identifiant unique de l'objet.
    public string ItemName = "Nouvel objet";

    // Description affichée dans l'inventaire.
    [TextArea(2, 4)]
    public string Description = "";

    // Sprite affiché dans l'inventaire.
    public Sprite Icon;

    // Rareté de l'objet.
    public LootRarity Rarity = LootRarity.Common;

    // Type d'effet appliqué au joueur.
    public ItemEffectType EffectType = ItemEffectType.Heal;

    // Valeur numérique de l'effet (ex: +2 vie, +10% vitesse).
    public float EffectValue = 1f;

    // Indique si l'effet a une durée limitée.
    public bool HasDuration = false;

    // Durée de l'effet en secondes (si HasDuration = true).
    public float EffectDuration = 5f;

    // Indique si l'objet a un temps de consommation.
    public bool HasConsumptionTime = false;

    // Temps de consommation en secondes (barre de progression).
    public float ConsumptionTime = 1.5f;
}
