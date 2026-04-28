using UnityEngine;

public enum ItemEffectType
{
    Heal,
    MaxHealthUp,
    Damage,
    Speed,
    AttackSpeed,
    Shield,
    NewHeroCartridge
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    public string ItemName = "Nouvel objet";
    [TextArea(2, 4)]
    public string Description = "";
    public Sprite Icon;
    public LootRarity Rarity = LootRarity.Common;
    public ItemEffectType EffectType = ItemEffectType.Heal;
    public float EffectValue = 1f;
    public bool HasDuration = false;
    public float EffectDuration = 5f;
    public bool HasConsumptionTime = false;
    public float ConsumptionTime = 1.5f;
}
