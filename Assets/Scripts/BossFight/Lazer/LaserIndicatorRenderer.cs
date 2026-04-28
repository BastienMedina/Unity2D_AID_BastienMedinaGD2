using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserIndicatorRenderer : MonoBehaviour
{
    [SerializeField] private TurnManager _turnManager;
    [SerializeField] private LaserSystem _laserSystem;
    [SerializeField] private Transform _container;
    [SerializeField] private float _cellSize = 1f;
    [SerializeField] private float _cellGap = 0.1f;
    [SerializeField] private Vector2 _gridOrigin = new Vector2(-2f, -2f);
    [SerializeField] private int _gridColumns = 5;
    [SerializeField] private int _gridRows = 5;
    [SerializeField] private float _indicatorSize = 0.4f;
    [SerializeField] private float _indicatorOffset = 0.8f;
    [SerializeField] private Sprite _indicatorSprite;
    [SerializeField] private float _punchDuration = 0.25f;
    [SerializeField] private float _punchScale = 1.5f;

    private GameObject[] _rowIndicators;
    private GameObject[] _columnIndicators;

    private void Awake() // Valide les références et crée les indicateurs
    {
        if (_turnManager == null)
            throw new MissingReferenceException($"[LaserIndicatorRenderer] {nameof(_turnManager)} non assigné.");

        if (_laserSystem == null)
            throw new MissingReferenceException($"[LaserIndicatorRenderer] {nameof(_laserSystem)} non assigné.");

        BuildIndicators();
    }

    private void Start() // Abonne le rafraîchissement et applique l'état initial
    {
        _turnManager.OnTurnProcessed.AddListener(RefreshDisplay);
        RefreshDisplay();
    }

    private void OnDisable() // Désabonne le rafraîchissement
    {
        if (_turnManager != null)
            _turnManager.OnTurnProcessed.RemoveListener(RefreshDisplay);
    }

    private void BuildIndicators() // Instancie un indicateur par ligne et par colonne
    {
        _rowIndicators    = new GameObject[_gridRows];
        _columnIndicators = new GameObject[_gridColumns];

        float step    = _cellSize + _cellGap;
        float leftEdge = _gridOrigin.x - _indicatorOffset;

        for (int row = 0; row < _gridRows; row++) // Indicateurs de lignes sur le bord gauche
        {
            GameObject indicator = CreateCircleSprite($"Indicator_Row_{row}", _container, 270f);
            float y = _gridOrigin.y + row * step;
            indicator.transform.position   = new Vector3(leftEdge, y, 0f);
            indicator.transform.localScale = Vector3.one * _indicatorSize;
            indicator.SetActive(false);
            _rowIndicators[row] = indicator;
        }

        float topEdge = _gridOrigin.y + (_gridRows - 1) * step + _indicatorOffset;

        for (int col = 0; col < _gridColumns; col++) // Indicateurs de colonnes sur le bord supérieur
        {
            GameObject indicator = CreateCircleSprite($"Indicator_Col_{col}", _container, 180f);
            float x = _gridOrigin.x + col * step;
            indicator.transform.position   = new Vector3(x, topEdge, 0f);
            indicator.transform.localScale = Vector3.one * _indicatorSize;
            indicator.SetActive(false);
            _columnIndicators[col] = indicator;
        }
    }

    private void RefreshDisplay() // Cache tout puis active les indicateurs du prochain pattern
    {
        for (int row = 0; row < _rowIndicators.Length; row++) // Désactive tous les indicateurs lignes
        {
            if (_rowIndicators[row] != null)
                _rowIndicators[row].SetActive(false);
        }

        for (int col = 0; col < _columnIndicators.Length; col++) // Désactive tous les indicateurs colonnes
        {
            if (_columnIndicators[col] != null)
                _columnIndicators[col].SetActive(false);
        }

        List<Vector2Int> nextCells = _laserSystem.GetNextLaserCells();
        HashSet<int> nextRows = new HashSet<int>();
        HashSet<int> nextCols = new HashSet<int>();

        for (int i = 0; i < nextCells.Count; i++) // Catalogue les lignes et colonnes actives
        {
            nextRows.Add(nextCells[i].y);
            nextCols.Add(nextCells[i].x);
        }

        for (int row = 0; row < _gridRows; row++) // Active l'indicateur si la ligne est entière
        {
            if (nextRows.Contains(row) && IsFullRow(nextCells, row) && _rowIndicators[row] != null)
            {
                _rowIndicators[row].SetActive(true);
                StartCoroutine(ScalePunchCoroutine(_rowIndicators[row].transform));
            }
        }

        for (int col = 0; col < _gridColumns; col++) // Active l'indicateur si la colonne est entière
        {
            if (nextCols.Contains(col) && IsFullColumn(nextCells, col) && _columnIndicators[col] != null)
            {
                _columnIndicators[col].SetActive(true);
                StartCoroutine(ScalePunchCoroutine(_columnIndicators[col].transform));
            }
        }
    }

    private IEnumerator ScalePunchCoroutine(Transform indicator) // Gonfle puis revient à la taille normale
    {
        float elapsed  = 0f;
        float half     = _punchDuration * 0.5f;
        float baseSize = _indicatorSize;
        float peakSize = _indicatorSize * _punchScale;

        while (elapsed < half) // Phase montante
        {
            elapsed += Time.deltaTime;
            indicator.localScale = Vector3.one * Mathf.Lerp(baseSize, peakSize, Mathf.Clamp01(elapsed / half));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half) // Phase descendante
        {
            elapsed += Time.deltaTime;
            indicator.localScale = Vector3.one * Mathf.Lerp(peakSize, baseSize, Mathf.Clamp01(elapsed / half));
            yield return null;
        }

        indicator.localScale = Vector3.one * baseSize;
    }

    private bool IsFullRow(List<Vector2Int> cells, int row) // Vérifie que toute la ligne est couverte
    {
        int count = 0;
        for (int i = 0; i < cells.Count; i++)
            if (cells[i].y == row) count++;
        return count >= _gridColumns;
    }

    private bool IsFullColumn(List<Vector2Int> cells, int col) // Vérifie que toute la colonne est couverte
    {
        int count = 0;
        for (int i = 0; i < cells.Count; i++)
            if (cells[i].x == col) count++;
        return count >= _gridRows;
    }

    private GameObject CreateCircleSprite(string objectName, Transform parent, float rotationZ = 0f) // Crée un sprite indicateur orienté
    {
        GameObject go = new GameObject(objectName);
        go.transform.SetParent(parent, false);
        go.transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = _indicatorSprite;
        sr.sortingOrder = 3;

        return go;
    }
}
