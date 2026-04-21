using System.Collections.Generic;
using UnityEngine;

// Génère un pool de patterns laser composés de lignes et colonnes entières.
// Chaque pattern active entre 1 et _maxStripsPerPattern rangées complètes,
// choisies aléatoirement parmi toutes les lignes et colonnes disponibles.
// S'exécute avant LaserSystem grâce à l'ordre d'exécution plus négatif.
[DefaultExecutionOrder(-30)]
[RequireComponent(typeof(LaserSystem))]
public class LaserPatternGenerator : MonoBehaviour
{
    // Nombre de patterns distincts à générer au lancement.
    [SerializeField] private int _patternCount = 4;

    // Nombre maximum de bandes (lignes ou colonnes) actives par pattern.
    [SerializeField] private int _maxStripsPerPattern = 2;

    // Colonnes de la grille de jeu.
    [SerializeField] private int _gridColumns = 5;

    // Lignes de la grille de jeu.
    [SerializeField] private int _gridRows = 5;

    // Génère les patterns et les injecte dans LaserSystem.
    private void Awake()
    {
        LaserSystem laserSystem = GetComponent<LaserSystem>();
        List<LaserPattern> patterns = GeneratePatterns();
        laserSystem.InjectPatterns(patterns);
    }

    // -------------------------------------------------------------------------
    // Génération
    // -------------------------------------------------------------------------

    // Crée _patternCount patterns composés de lignes/colonnes entières.
    private List<LaserPattern> GeneratePatterns()
    {
        List<LaserPattern> patterns = new List<LaserPattern>(_patternCount);

        for (int i = 0; i < _patternCount; i++)
        {
            LaserPattern pattern = ScriptableObject.CreateInstance<LaserPattern>();
            pattern.name = $"GeneratedPattern_{i}";

            List<Vector2Int> cells = GenerateStripCells();
            pattern.Init(i, cells);

            patterns.Add(pattern);
        }

        return patterns;
    }

    // Génère les cellules d'un pattern en sélectionnant des lignes ou colonnes entières.
    // Chaque bande occupe l'intégralité de ses cellules pour garantir le rendu en capsule.
    private List<Vector2Int> GenerateStripCells()
    {
        // Compte le nombre de bandes actives dans ce pattern (au moins 1).
        int stripCount = Random.Range(1, _maxStripsPerPattern + 1);

        // Construit la liste des bandes candidates : lignes (isRow=true) et colonnes (isRow=false).
        List<(bool isRow, int index)> candidates = new List<(bool, int)>();

        for (int row = 0; row < _gridRows; row++)
            candidates.Add((true, row));

        for (int col = 0; col < _gridColumns; col++)
            candidates.Add((false, col));

        // Mélange Fisher-Yates pour une sélection uniforme parmi les bandes.
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        // Sélectionne les N premières bandes après mélange.
        int take = Mathf.Min(stripCount, candidates.Count);

        // Accumule les cellules de chaque bande sélectionnée.
        List<Vector2Int> cells = new List<Vector2Int>();
        HashSet<Vector2Int> seen = new HashSet<Vector2Int>();

        for (int s = 0; s < take; s++)
        {
            (bool isRow, int index) = candidates[s];

            if (isRow)
            {
                // Ajoute toutes les cellules de cette ligne (row = index).
                for (int col = 0; col < _gridColumns; col++)
                {
                    Vector2Int cell = new Vector2Int(col, index);
                    if (seen.Add(cell))
                        cells.Add(cell);
                }
            }
            else
            {
                // Ajoute toutes les cellules de cette colonne (col = index).
                for (int row = 0; row < _gridRows; row++)
                {
                    Vector2Int cell = new Vector2Int(index, row);
                    if (seen.Add(cell))
                        cells.Add(cell);
                }
            }
        }

        return cells;
    }
}
