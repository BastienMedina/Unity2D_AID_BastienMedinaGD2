using System;
using System.Collections.Generic;
using UnityEngine;

// Entrée de la table de loot avec objet et poids de rareté.
[Serializable]
public class LootEntry
{
    // Objet associé à cette entrée de la table.
    public ItemData Item;

    // Poids de tirage (plus élevé = plus fréquent).
    public float Weight = 1f;
}

// Table de loot basée sur des ItemData pondérés par rareté.
[CreateAssetMenu(fileName = "LootTable", menuName = "Game/Loot Table")]
public class LootTable : ScriptableObject
{
    // Liste des entrées configurées depuis l'Inspector.
    [SerializeField] private List<LootEntry> _entries = new List<LootEntry>();

    // Poids de tirage attribué aux objets de rareté Common.
    [SerializeField] private float _commonWeight = 60f;

    // Poids de tirage attribué aux objets de rareté Uncommon.
    [SerializeField] private float _uncommonWeight = 30f;

    // Poids de tirage attribué aux objets de rareté Rare.
    [SerializeField] private float _rareWeight = 8f;

    // Poids de tirage attribué aux objets de rareté Legendary.
    [SerializeField] private float _legendaryWeight = 2f;

    // Construit la table automatiquement depuis Resources/Items.
    public void BuildFromResources()
    {
        // Charge tous les ItemData du dossier Resources/Items.
        ItemData[] allItems = Resources.LoadAll<ItemData>("Items");

        // Vide la liste avant de la reconstruire entièrement.
        _entries.Clear();

        // Crée une entrée pondérée pour chaque ItemData trouvé.
        foreach (ItemData item in allItems)
        {
            // Détermine le poids selon la rareté de l'objet.
            float weight = item.Rarity switch
            {
                LootRarity.Common    => _commonWeight,
                LootRarity.Uncommon  => _uncommonWeight,
                LootRarity.Rare      => _rareWeight,
                LootRarity.Legendary => _legendaryWeight,
                _                    => _commonWeight
            };

            // Ajoute l'entrée pondérée à la table de loot.
            _entries.Add(new LootEntry { Item = item, Weight = weight });
        }
    }

    // Tire un ItemData aléatoire selon les poids configurés.
    public ItemData Roll()
    {
        // Reconstruit la table depuis Resources si elle est vide.
        if (_entries == null || _entries.Count == 0)
            BuildFromResources();

        // Abandonne si la table est toujours vide après reconstruction.
        if (_entries == null || _entries.Count == 0)
        {
            Debug.LogWarning("[LootTable] Aucune entrée disponible après BuildFromResources.");
            return null;
        }

        // Calcule le poids total de toutes les entrées disponibles.
        float totalWeight = 0f;

        // Additionne chaque poids pour obtenir le total de référence.
        foreach (LootEntry entry in _entries)
            totalWeight += entry.Weight;

        // Tire un nombre aléatoire dans l'intervalle du poids total.
        float roll = UnityEngine.Random.Range(0f, totalWeight);

        // Parcourt les entrées pour trouver celle du tirage.
        float cumulative = 0f;

        // Vérifie chaque entrée en accumulant les poids progressivement.
        foreach (LootEntry entry in _entries)
        {
            // Ajoute le poids de cette entrée au cumul courant.
            cumulative += entry.Weight;

            // Retourne l'ItemData si le seuil cumulé est atteint.
            if (roll <= cumulative)
                return entry.Item;
        }

        // Retourne le dernier item en cas d'imprécision flottante.
        return _entries[_entries.Count - 1].Item;
    }
}
