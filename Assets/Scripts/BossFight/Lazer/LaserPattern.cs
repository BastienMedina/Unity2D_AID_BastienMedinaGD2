using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LaserPattern", menuName = "MiniGame/LaserPattern")]
public class LaserPattern : ScriptableObject
{
    [SerializeField] private int _patternIndex;
    [SerializeField] private List<Vector2Int> _activeCells = new List<Vector2Int>();

    public int PatternIndex => _patternIndex;
    public IReadOnlyList<Vector2Int> ActiveCells => _activeCells;

    public void Init(int patternIndex, List<Vector2Int> cells) // Initialise le pattern par code
    {
        _patternIndex = patternIndex;
        _activeCells  = new List<Vector2Int>(cells);
    }
}
