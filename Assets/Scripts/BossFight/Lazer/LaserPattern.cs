using System.Collections.Generic;
using UnityEngine;

// Données d'un pattern laser configurables via l'éditeur.
[CreateAssetMenu(fileName = "LaserPattern", menuName = "MiniGame/LaserPattern")]
public class LaserPattern : ScriptableObject
{
    // Identifiant numérique de ce pattern dans le design.
    [SerializeField] private int _patternIndex;

    // Cellules de la grille actives pour ce pattern.
    [SerializeField] private List<Vector2Int> _activeCells = new List<Vector2Int>();

    // Expose l'identifiant du pattern en lecture seule.
    public int PatternIndex => _patternIndex;

    // Expose les cellules actives en lecture seule.
    public IReadOnlyList<Vector2Int> ActiveCells => _activeCells;

    /// <summary>Initialise les données d'un pattern généré par code.</summary>
    public void Init(int patternIndex, List<Vector2Int> cells)
    {
        _patternIndex = patternIndex;
        _activeCells = new List<Vector2Int>(cells);
    }
}
