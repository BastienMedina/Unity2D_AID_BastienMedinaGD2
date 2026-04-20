using System.Collections.Generic;
using UnityEngine;

// Génère un pool de patterns laser aléatoires et l'injecte dans LaserSystem.
// S'exécute avant LaserSystem grâce à l'ordre d'exécution négatif.
[DefaultExecutionOrder(-20)]
[RequireComponent(typeof(LaserSystem))]
public class LaserPatternGenerator : MonoBehaviour
{
    // Nombre de patterns à générer au lancement
    [SerializeField] private int _patternCount = 4;

    // Nombre de cellules actives par pattern
    [SerializeField] private int _cellsPerPattern = 4;

    // Colonnes de la grille de jeu
    [SerializeField] private int _gridColumns = 5;

    // Lignes de la grille de jeu
    [SerializeField] private int _gridRows = 5;

    // Génère les patterns aléatoires et les injecte dans LaserSystem avant son Awake
    private void Awake()
    {
        LaserSystem laserSystem = GetComponent<LaserSystem>();
        List<LaserPattern> patterns = GeneratePatterns();
        laserSystem.InjectPatterns(patterns);
    }

    // -------------------------------------------------------------------------
    // Génération
    // -------------------------------------------------------------------------

    // Crée _patternCount patterns uniques avec des cellules distribuées aléatoirement
    private List<LaserPattern> GeneratePatterns()
    {
        List<LaserPattern> patterns = new List<LaserPattern>(_patternCount);

        for (int i = 0; i < _patternCount; i++)
        {
            LaserPattern pattern = ScriptableObject.CreateInstance<LaserPattern>();
            pattern.name = $"GeneratedPattern_{i}";

            List<Vector2Int> cells = PickRandomCells(_cellsPerPattern);
            pattern.Init(i, cells);

            patterns.Add(pattern);
        }

        return patterns;
    }

    // Pioche _count cellules uniques aléatoirement dans la grille
    private List<Vector2Int> PickRandomCells(int count)
    {
        // Construit la liste de toutes les cellules disponibles
        List<Vector2Int> allCells = new List<Vector2Int>(_gridColumns * _gridRows);

        for (int col = 0; col < _gridColumns; col++)
        {
            for (int row = 0; row < _gridRows; row++)
            {
                allCells.Add(new Vector2Int(col, row));
            }
        }

        // Mélange Fisher-Yates pour une sélection uniforme
        for (int i = allCells.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (allCells[i], allCells[j]) = (allCells[j], allCells[i]);
        }

        // Prend les N premières cellules après mélange
        int take = Mathf.Min(count, allCells.Count);
        return allCells.GetRange(0, take);
    }
}
