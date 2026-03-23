using System;
using System.Collections.Generic;
using UnityEngine;

// Définit les entrées de butin avec leurs poids de rareté
[CreateAssetMenu(fileName = "LootTable", menuName = "Loot/LootTable")]
public class LootTable : ScriptableObject
{
    // Liste des entrées de butin configurables depuis l'inspecteur
    [SerializeField] private List<LootEntry> _entries = new List<LootEntry>();

    // Sélectionne un préfab aléatoire pondéré parmi les entrées
    public GameObject Roll()
    {
        // Retourne null si aucune entrée n'est configurée dans la table
        if (_entries == null || _entries.Count == 0)
        {
            return null;
        }

        // Calcule la somme totale des poids de toutes les entrées
        float totalWeight = 0f;

        // Additionne chaque poids pour obtenir le total de référence
        foreach (LootEntry entry in _entries)
        {
            totalWeight += entry.Weight;
        }

        // Tire un nombre aléatoire dans l'intervalle du poids total
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);

        // Parcourt les entrées pour trouver celle correspondant au tirage
        float cumulative = 0f;

        // Vérifie chaque entrée en accumulant les poids successifs
        foreach (LootEntry entry in _entries)
        {
            // Ajoute le poids courant au total cumulé
            cumulative += entry.Weight;

            // Retourne le préfab si le tirage tombe dans cet intervalle
            if (randomValue <= cumulative)
            {
                return entry.Prefab;
            }
        }

        // Retourne le dernier préfab en cas d'imprécision flottante
        return _entries[_entries.Count - 1].Prefab;
    }
}

// Représente une entrée de butin avec son préfab, poids et rareté
[Serializable]
public class LootEntry
{
    // Préfab instancié si cette entrée est sélectionnée par le Roll
    [SerializeField] private GameObject _prefab;

    // Poids relatif déterminant la probabilité de sélection
    [SerializeField] private float _weight = 1f;

    // Niveau de rareté associé à cette entrée de butin
    [SerializeField] private LootRarity _rarity = LootRarity.Common;

    // Expose le préfab en lecture seule pour le système de loot
    public GameObject Prefab => _prefab;

    // Expose le poids en lecture seule pour le calcul de Roll
    public float Weight => _weight;

    // Expose la rareté en lecture seule pour les abonnés externes
    public LootRarity Rarity => _rarity;
}
