using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserRenderer : MonoBehaviour
{
    [SerializeField] private TurnManager _turnManager;
    [SerializeField] private LaserSystem _laserSystem;
    [SerializeField] private Transform _container;
    [SerializeField] private float _cellSize = 1f;
    [SerializeField] private float _cellGap = 0.1f;
    [SerializeField] private Vector2 _gridOrigin = new Vector2(-2f, -2f);
    [SerializeField] private int _gridColumns = 5;
    [SerializeField] private int _gridRows = 5;
    [SerializeField] private Sprite _laserSprite;
    [SerializeField] private float _blinkAlphaMin = 0.35f;
    [SerializeField] private float _blinkAlphaMax = 1f;
    [SerializeField] private float _blinkPeriod = 1.2f;

    private GameObject[] _rowCapsules;
    private GameObject[] _columnCapsules;
    private SpriteRenderer[] _rowRenderers;
    private SpriteRenderer[] _columnRenderers;
    private Coroutine _blinkCoroutine;

    private void Awake() // Valide les refs et construit le pool de capsules
    {
        if (_turnManager == null)
            throw new MissingReferenceException($"[LaserRenderer] {nameof(_turnManager)} non assigné.");
        if (_laserSystem == null)
            throw new MissingReferenceException($"[LaserRenderer] {nameof(_laserSystem)} non assigné.");
        if (_laserSprite == null)
            throw new MissingReferenceException($"[LaserRenderer] {nameof(_laserSprite)} non assigné.");

        BuildCapsules();
    }

    private void Start() // Abonne le rafraîchissement et affiche l'état initial
    {
        _turnManager.OnTurnProcessed.AddListener(RefreshDisplay);
        RefreshDisplay();
    }

    private void OnDisable() // Désabonne et arrête le blink
    {
        if (_turnManager != null)
            _turnManager.OnTurnProcessed.RemoveListener(RefreshDisplay);
        StopBlink();
    }

    private void BuildCapsules() // Instancie une capsule par ligne et par colonne
    {
        _rowCapsules     = new GameObject[_gridRows];
        _columnCapsules  = new GameObject[_gridColumns];
        _rowRenderers    = new SpriteRenderer[_gridRows];
        _columnRenderers = new SpriteRenderer[_gridColumns];

        float step       = _cellSize + _cellGap;
        float fullWidth  = _gridColumns * _cellSize + (_gridColumns - 1) * _cellGap;
        float fullHeight = _gridRows    * _cellSize + (_gridRows    - 1) * _cellGap;
        float spriteW    = _laserSprite.bounds.size.x;
        float spriteH    = _laserSprite.bounds.size.y;

        for (int row = 0; row < _gridRows; row++) // Capsules horizontales par ligne
        {
            SpriteRenderer sr;
            GameObject capsule = CreateFilledSprite($"Laser_Row_{row}", _container, out sr);
            float x = _gridOrigin.x + (_gridColumns - 1) * step * 0.5f;
            float y = _gridOrigin.y + row * step;
            capsule.transform.position   = new Vector3(x, y, 0f);
            capsule.transform.localScale = new Vector3(fullWidth / spriteW, _cellSize / spriteH, 1f); // Scale sur la taille native
            capsule.SetActive(false);
            _rowCapsules[row]  = capsule;
            _rowRenderers[row] = sr;
        }

        for (int col = 0; col < _gridColumns; col++) // Capsules verticales par colonne (rotation 90°)
        {
            SpriteRenderer sr;
            GameObject capsule = CreateFilledSprite($"Laser_Col_{col}", _container, out sr);
            float x = _gridOrigin.x + col * step;
            float y = _gridOrigin.y + (_gridRows - 1) * step * 0.5f;
            capsule.transform.position   = new Vector3(x, y, 0f);
            capsule.transform.rotation   = Quaternion.Euler(0f, 0f, 90f);
            capsule.transform.localScale = new Vector3(fullHeight / spriteW, _cellSize / spriteH, 1f);
            capsule.SetActive(false);
            _columnCapsules[col]  = capsule;
            _columnRenderers[col] = sr;
        }
    }

    private void RefreshDisplay() // Désactive tout puis active les capsules du pattern courant
    {
        StopBlink();

        for (int i = 0; i < _rowCapsules.Length; i++) // Désactive toutes les capsules lignes
            if (_rowCapsules[i] != null) _rowCapsules[i].SetActive(false);

        for (int i = 0; i < _columnCapsules.Length; i++) // Désactive toutes les capsules colonnes
            if (_columnCapsules[i] != null) _columnCapsules[i].SetActive(false);

        List<Vector2Int> activeCells = _laserSystem.GetCurrentLaserCells();
        if (activeCells == null || activeCells.Count == 0) return;

        for (int row = 0; row < _gridRows; row++) // Active la capsule si la ligne est entière
        {
            if (IsFullRow(activeCells, row) && _rowCapsules[row] != null)
                _rowCapsules[row].SetActive(true);
        }

        for (int col = 0; col < _gridColumns; col++) // Active la capsule si la colonne est entière
        {
            if (IsFullColumn(activeCells, col) && _columnCapsules[col] != null)
                _columnCapsules[col].SetActive(true);
        }

        _blinkCoroutine = StartCoroutine(BlinkCoroutine());
    }

    private bool IsFullRow(List<Vector2Int> cells, int row) // Vérifie que toute la ligne est dans les cellules
    {
        int count = 0;
        for (int i = 0; i < cells.Count; i++)
            if (cells[i].y == row) count++;
        return count >= _gridColumns;
    }

    private bool IsFullColumn(List<Vector2Int> cells, int col) // Vérifie que toute la colonne est dans les cellules
    {
        int count = 0;
        for (int i = 0; i < cells.Count; i++)
            if (cells[i].x == col) count++;
        return count >= _gridRows;
    }

    private IEnumerator BlinkCoroutine() // Fait osciller l'alpha des capsules en boucle
    {
        float time = 0f;
        while (true)
        {
            time += Time.deltaTime;
            float t     = (Mathf.Sin(time * (2f * Mathf.PI / _blinkPeriod)) + 1f) * 0.5f; // Oscillation sin → [0,1]
            float alpha = Mathf.Lerp(_blinkAlphaMin, _blinkAlphaMax, t);
            ApplyAlphaToActiveCapsules(alpha);
            yield return null;
        }
    }

    private void ApplyAlphaToActiveCapsules(float alpha) // Applique l'alpha à toutes les capsules actives
    {
        for (int i = 0; i < _rowRenderers.Length; i++) // Capsules lignes actives
        {
            if (_rowRenderers[i] != null && _rowCapsules[i] != null && _rowCapsules[i].activeSelf)
            {
                Color c = _rowRenderers[i].color;
                c.a = alpha;
                _rowRenderers[i].color = c;
            }
        }

        for (int i = 0; i < _columnRenderers.Length; i++) // Capsules colonnes actives
        {
            if (_columnRenderers[i] != null && _columnCapsules[i] != null && _columnCapsules[i].activeSelf)
            {
                Color c = _columnRenderers[i].color;
                c.a = alpha;
                _columnRenderers[i].color = c;
            }
        }
    }

    private void StopBlink() // Arrête la coroutine et remet l'alpha à 1
    {
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }
        ApplyAlphaToActiveCapsules(1f);
    }

    private GameObject CreateFilledSprite(string objectName, Transform parent, out SpriteRenderer sr) // Crée un GO avec le sprite laser
    {
        GameObject go = new GameObject(objectName);
        go.transform.SetParent(parent, false);
        sr              = go.AddComponent<SpriteRenderer>();
        sr.sprite       = _laserSprite;
        sr.sortingOrder = 2;
        return go;
    }
}
