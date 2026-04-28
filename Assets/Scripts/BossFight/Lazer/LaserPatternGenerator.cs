using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-30)]
[RequireComponent(typeof(LaserSystem))]
public class LaserPatternGenerator : MonoBehaviour
{
    [SerializeField] private int _patternCount = 4;
    [SerializeField] private int _maxStripsPerPattern = 2;
    [SerializeField] private int _gridColumns = 5;
    [SerializeField] private int _gridRows = 5;

    private void Awake() // Génère les patterns et les injecte dans LaserSystem
    {
        LaserSystem laserSystem = GetComponent<LaserSystem>();
        List<LaserPattern> patterns = GeneratePatterns();
        laserSystem.InjectPatterns(patterns);
    }

    private List<LaserPattern> GeneratePatterns() // Crée N patterns de bandes aléatoires
    {
        List<LaserPattern> patterns = new List<LaserPattern>(_patternCount);

        for (int i = 0; i < _patternCount; i++) // Génère chaque pattern individuellement
        {
            LaserPattern pattern = ScriptableObject.CreateInstance<LaserPattern>();
            pattern.name = $"GeneratedPattern_{i}";
            pattern.Init(i, GenerateStripCells());
            patterns.Add(pattern);
        }

        return patterns;
    }

    private List<Vector2Int> GenerateStripCells() // Sélectionne des lignes ou colonnes entières
    {
        int stripCount = Random.Range(1, _maxStripsPerPattern + 1);

        List<(bool isRow, int index)> candidates = new List<(bool, int)>(); // Toutes les bandes candidates

        for (int row = 0; row < _gridRows; row++)
            candidates.Add((true, row));

        for (int col = 0; col < _gridColumns; col++)
            candidates.Add((false, col));

        for (int i = candidates.Count - 1; i > 0; i--) // Mélange Fisher-Yates
        {
            int j = Random.Range(0, i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        int take = Mathf.Min(stripCount, candidates.Count);
        List<Vector2Int> cells = new List<Vector2Int>();
        HashSet<Vector2Int> seen = new HashSet<Vector2Int>();

        for (int s = 0; s < take; s++) // Accumule les cellules de chaque bande
        {
            (bool isRow, int index) = candidates[s];

            if (isRow)
            {
                for (int col = 0; col < _gridColumns; col++) // Ajoute toute la ligne
                {
                    Vector2Int cell = new Vector2Int(col, index);
                    if (seen.Add(cell)) cells.Add(cell);
                }
            }
            else
            {
                for (int row = 0; row < _gridRows; row++) // Ajoute toute la colonne
                {
                    Vector2Int cell = new Vector2Int(index, row);
                    if (seen.Add(cell)) cells.Add(cell);
                }
            }
        }

        return cells;
    }
}
