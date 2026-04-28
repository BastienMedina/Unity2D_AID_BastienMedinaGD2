using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class LaserSystem : MonoBehaviour
{
    [SerializeField] private TurnManager _turnManager;
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private List<LaserPattern> _patterns = new List<LaserPattern>();
    [SerializeField] private AudioClip _advanceLaserClip;

    public UnityEvent OnPlayerHitByLaser = new UnityEvent();

    private int _currentPatternIndex;
    private int _previousPatternIndex = -1;
    private int _nextPatternIndex = -1;
    private List<Vector2Int> _activeLaserCells = new List<Vector2Int>();

    private bool HasValidPatterns => _patterns != null && _patterns.Count > 0;

    private void Awake() // Valide les refs, s'abonne et charge le premier pattern
    {
        if (_turnManager == null)
            throw new MissingReferenceException($"[LaserSystem] {nameof(_turnManager)} non assigné.");

        if (_gridManager == null)
            throw new MissingReferenceException($"[LaserSystem] {nameof(_gridManager)} non assigné.");

        _turnManager.OnEvaluateLaser.AddListener(HandleEvaluateLaser);
        _turnManager.OnAdvanceLaser.AddListener(HandleAdvanceLaser);

        _currentPatternIndex  = 0;
        _previousPatternIndex = -1;
        _nextPatternIndex     = PickNextIndex(_currentPatternIndex, _previousPatternIndex); // Pré-calcule le prochain index
        RebuildActiveCells();
    }

    private void OnDestroy() // Désabonne les écouteurs de tour
    {
        if (_turnManager != null)
        {
            _turnManager.OnEvaluateLaser.RemoveListener(HandleEvaluateLaser);
            _turnManager.OnAdvanceLaser.RemoveListener(HandleAdvanceLaser);
        }
    }

    public void InjectPatterns(List<LaserPattern> patterns) // Injecte les patterns générés par code
    {
        _patterns = patterns;
    }

    public bool EvaluatePosition(Vector2Int playerPos) // Vérifie si le joueur est dans le laser
    {
        if (!HasValidPatterns)
            return false;

        LaserPattern current = GetCurrentPattern();
        if (current == null || current.ActiveCells == null)
            return false;

        bool isHit = current.ActiveCells.Contains(playerPos);
        if (isHit)
            OnPlayerHitByLaser.Invoke();

        return isHit;
    }

    public List<Vector2Int> GetCurrentLaserCells() // Retourne une copie des cellules actives
    {
        return new List<Vector2Int>(_activeLaserCells);
    }

    public List<Vector2Int> GetNextLaserCells() // Retourne les cellules du prochain pattern
    {
        if (!HasValidPatterns || _patterns.Count < 2)
            return new List<Vector2Int>();

        if (_nextPatternIndex < 0 || _nextPatternIndex >= _patterns.Count)
            return new List<Vector2Int>();

        LaserPattern next = _patterns[_nextPatternIndex];
        if (next == null || next.ActiveCells == null)
            return new List<Vector2Int>();

        return new List<Vector2Int>(next.ActiveCells);
    }

    private void HandleEvaluateLaser() // Évalue la position du joueur ce tour
    {
        if (!HasValidPatterns)
            return;

        EvaluatePosition(_gridManager.GetPlayerPosition());
    }

    private void HandleAdvanceLaser() // Avance vers un pattern aléatoire différent
    {
        if (!HasValidPatterns)
            return;

        AudioManager.Instance?.PlaySFX(_advanceLaserClip);

        if (_patterns.Count == 1) // Un seul pattern disponible, reste en place
        {
            _currentPatternIndex = 0;
            RebuildActiveCells();
            return;
        }

        _previousPatternIndex = _currentPatternIndex;
        _currentPatternIndex  = _nextPatternIndex;
        _nextPatternIndex     = PickNextIndex(_currentPatternIndex, _previousPatternIndex); // Pré-calcule le suivant
        RebuildActiveCells();
    }

    private LaserPattern GetCurrentPattern() // Retourne le pattern courant ou null
    {
        if (_currentPatternIndex < 0 || _currentPatternIndex >= _patterns.Count)
            return null;

        return _patterns[_currentPatternIndex];
    }

    private int PickNextIndex(int excludeCurrent, int excludePrevious) // Choisit un index aléatoire valide
    {
        if (_patterns.Count == 1)
            return 0;

        List<int> candidates = new List<int>(_patterns.Count);
        for (int i = 0; i < _patterns.Count; i++) // Exclut courant et précédent
        {
            if (i == excludeCurrent || i == excludePrevious) continue;
            candidates.Add(i);
        }

        if (candidates.Count == 0) // Fallback si tous exclus
        {
            for (int i = 0; i < _patterns.Count; i++)
                if (i != excludeCurrent) candidates.Add(i);
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    private void RebuildActiveCells() // Recharge les cellules du pattern courant
    {
        _activeLaserCells.Clear();
        LaserPattern current = GetCurrentPattern();

        if (current == null || current.ActiveCells == null)
            return;

        foreach (Vector2Int cell in current.ActiveCells) // Copie les cellules dans la liste mutable
            _activeLaserCells.Add(cell);
    }
}
