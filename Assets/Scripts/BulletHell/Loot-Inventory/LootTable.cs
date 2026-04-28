using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LootEntry
{
    public ItemData Item;
    public float Weight = 1f;
}

[CreateAssetMenu(fileName = "LootTable", menuName = "Game/Loot Table")]
public class LootTable : ScriptableObject
{
    [SerializeField] private List<LootEntry> _entries = new List<LootEntry>();
    [SerializeField] private float _commonWeight    = 60f;
    [SerializeField] private float _uncommonWeight  = 30f;
    [SerializeField] private float _rareWeight      = 8f;
    [SerializeField] private float _legendaryWeight = 2f;

    public void BuildFromResources() // Charge tous les ItemData depuis Resources/Items
    {
        ItemData[] allItems = Resources.LoadAll<ItemData>("Items");
        _entries.Clear();

        foreach (ItemData item in allItems) // Crée une entrée pondérée par rareté
        {
            float weight = item.Rarity switch
            {
                LootRarity.Common    => _commonWeight,
                LootRarity.Uncommon  => _uncommonWeight,
                LootRarity.Rare      => _rareWeight,
                LootRarity.Legendary => _legendaryWeight,
                _                    => _commonWeight
            };

            _entries.Add(new LootEntry { Item = item, Weight = weight });
        }
    }

    public ItemData Roll() // Tire un ItemData aléatoire selon les poids
    {
        if (_entries == null || _entries.Count == 0) // Reconstruit si table vide
            BuildFromResources();

        if (_entries == null || _entries.Count == 0)
        {
            Debug.LogWarning("[LootTable] Aucune entrée disponible.");
            return null;
        }

        float totalWeight = 0f;
        foreach (LootEntry entry in _entries) // Calcule le poids total
            totalWeight += entry.Weight;

        float roll       = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (LootEntry entry in _entries) // Parcourt jusqu'au seuil atteint
        {
            cumulative += entry.Weight;
            if (roll <= cumulative)
                return entry.Item;
        }

        return _entries[_entries.Count - 1].Item; // Fallback dernier item
    }
}
