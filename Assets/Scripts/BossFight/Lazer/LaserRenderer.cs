using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Affiche les bandes laser actives du pattern courant.
// Chaque ligne ou colonne entièrement couverte par le pattern
// est représentée par une capsule pleine traversant toute la grille.
public class LaserRenderer : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Références
    // -------------------------------------------------------------------------

    // Référence au gestionnaire de tour pour s'abonner.
    [SerializeField] private TurnManager _turnManager;

    // Référence au système laser pour lire les cellules actives.
    [SerializeField] private LaserSystem _laserSystem;

    // Conteneur parent des capsules laser.
    [SerializeField] private Transform _container;

    // -------------------------------------------------------------------------
    // Paramètres de mise en page de la grille
    // -------------------------------------------------------------------------

    // Taille d'une cellule en unités monde Unity.
    [SerializeField] private float _cellSize = 1f;

    // Espacement entre les cellules en unités monde.
    [SerializeField] private float _cellGap = 0.1f;

    // Position monde du centre de la cellule (0, 0).
    [SerializeField] private Vector2 _gridOrigin = new Vector2(-2f, -2f);

    // Nombre de colonnes de la grille pour le rendu laser.
    [SerializeField] private int _gridColumns = 5;

    // Nombre de lignes de la grille pour le rendu laser.
    [SerializeField] private int _gridRows = 5;

    // Sprite utilisé pour les capsules laser — doit être assigné dans l'Inspector.
    [SerializeField] private Sprite _laserSprite;

    // -------------------------------------------------------------------------
    // Paramètres de feedback — blink alpha
    // -------------------------------------------------------------------------

    // Alpha minimal pendant le blink (0 = invisible).
    [SerializeField] private float _blinkAlphaMin = 0.35f;

    // Alpha maximal pendant le blink (1 = opaque).
    [SerializeField] private float _blinkAlphaMax = 1f;

    // Durée d'un cycle complet de blink (secondes).
    [SerializeField] private float _blinkPeriod = 1.2f;

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    // Capsule pour chaque ligne, indexée par numéro de ligne.
    private GameObject[] _rowCapsules;

    // Capsule pour chaque colonne, indexée par numéro de colonne.
    private GameObject[] _columnCapsules;

    // SpriteRenderer de chaque capsule de ligne.
    private SpriteRenderer[] _rowRenderers;

    // SpriteRenderer de chaque capsule de colonne.
    private SpriteRenderer[] _columnRenderers;

    // Coroutine de blink globale.
    private Coroutine _blinkCoroutine;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (_turnManager == null)
            throw new MissingReferenceException($"[LaserRenderer] La référence {nameof(_turnManager)} n'est pas assignée.");

        if (_laserSystem == null)
            throw new MissingReferenceException($"[LaserRenderer] La référence {nameof(_laserSystem)} n'est pas assignée.");

        if (_laserSprite == null)
            throw new MissingReferenceException($"[LaserRenderer] La référence {nameof(_laserSprite)} n'est pas assignée.");

        BuildCapsules();
    }

    private void Start()
    {
        _turnManager.OnTurnProcessed.AddListener(RefreshDisplay);
        RefreshDisplay();
    }

    private void OnDisable()
    {
        if (_turnManager != null)
            _turnManager.OnTurnProcessed.RemoveListener(RefreshDisplay);

        StopBlink();
    }

    // -------------------------------------------------------------------------
    // Construction du pool de capsules
    // -------------------------------------------------------------------------

    // Instancie une capsule pleine par ligne et par colonne, toutes cachées.
    private void BuildCapsules()
    {
        _rowCapsules    = new GameObject[_gridRows];
        _columnCapsules = new GameObject[_gridColumns];
        _rowRenderers   = new SpriteRenderer[_gridRows];
        _columnRenderers= new SpriteRenderer[_gridColumns];

        float step       = _cellSize + _cellGap;
        float fullWidth  = _gridColumns * _cellSize + (_gridColumns - 1) * _cellGap;
        float fullHeight = _gridRows    * _cellSize + (_gridRows    - 1) * _cellGap;

        // Taille monde native du sprite (pixels / PPU) — base pour le calcul d'échelle.
        float spriteW = _laserSprite.bounds.size.x;
        float spriteH = _laserSprite.bounds.size.y;

        // Capsule horizontale — sprite déjà orienté en largeur, pas de rotation.
        for (int row = 0; row < _gridRows; row++)
        {
            SpriteRenderer sr;
            GameObject capsule = CreateFilledSprite($"Laser_Row_{row}", _container, out sr);

            float x = _gridOrigin.x + (_gridColumns - 1) * step * 0.5f;
            float y = _gridOrigin.y + row * step;

            capsule.transform.position   = new Vector3(x, y, 0f);
            // Divise la taille voulue par la taille native du sprite pour obtenir le bon scale.
            capsule.transform.localScale = new Vector3(fullWidth / spriteW, _cellSize / spriteH, 1f);
            capsule.SetActive(false);

            _rowCapsules[row]   = capsule;
            _rowRenderers[row]  = sr;
        }

        // Capsule verticale — rotation 90° pour orienter le sprite horizontal en vertical.
        for (int col = 0; col < _gridColumns; col++)
        {
            SpriteRenderer sr;
            GameObject capsule = CreateFilledSprite($"Laser_Col_{col}", _container, out sr);

            float x = _gridOrigin.x + col * step;
            float y = _gridOrigin.y + (_gridRows - 1) * step * 0.5f;

            capsule.transform.position = new Vector3(x, y, 0f);
            capsule.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            // Après rotation 90° : localScale.x → hauteur monde, localScale.y → largeur monde.
            capsule.transform.localScale = new Vector3(fullHeight / spriteW, _cellSize / spriteH, 1f);
            capsule.SetActive(false);

            _columnCapsules[col]   = capsule;
            _columnRenderers[col]  = sr;
        }
    }

    // -------------------------------------------------------------------------
    // Rafraîchissement de l'affichage
    // -------------------------------------------------------------------------

    // Cache toutes les capsules puis active celles correspondant au pattern courant.
    private void RefreshDisplay()
    {
        // Arrête le blink précédent avant de tout reconfigurer.
        StopBlink();

        // Désactive toutes les capsules.
        for (int i = 0; i < _rowCapsules.Length; i++)
            if (_rowCapsules[i] != null) _rowCapsules[i].SetActive(false);

        for (int i = 0; i < _columnCapsules.Length; i++)
            if (_columnCapsules[i] != null) _columnCapsules[i].SetActive(false);

        List<Vector2Int> activeCells = _laserSystem.GetCurrentLaserCells();
        if (activeCells == null || activeCells.Count == 0) return;

        // Détecte les lignes entières couvertes et active leur capsule.
        for (int row = 0; row < _gridRows; row++)
        {
            if (IsFullRow(activeCells, row) && _rowCapsules[row] != null)
                _rowCapsules[row].SetActive(true);
        }

        // Détecte les colonnes entières couvertes et active leur capsule.
        for (int col = 0; col < _gridColumns; col++)
        {
            if (IsFullColumn(activeCells, col) && _columnCapsules[col] != null)
                _columnCapsules[col].SetActive(true);
        }

        // Démarre le blink sur toutes les capsules actives.
        _blinkCoroutine = StartCoroutine(BlinkCoroutine());
    }

    // Retourne vrai si toutes les colonnes de cette ligne sont dans les cellules actives.
    private bool IsFullRow(List<Vector2Int> cells, int row)
    {
        int count = 0;
        for (int i = 0; i < cells.Count; i++)
            if (cells[i].y == row) count++;
        return count >= _gridColumns;
    }

    // Retourne vrai si toutes les lignes de cette colonne sont dans les cellules actives.
    private bool IsFullColumn(List<Vector2Int> cells, int col)
    {
        int count = 0;
        for (int i = 0; i < cells.Count; i++)
            if (cells[i].x == col) count++;
        return count >= _gridRows;
    }

    // -------------------------------------------------------------------------
    // Feedback — blink alpha
    // -------------------------------------------------------------------------

    // Fait osciller l'alpha de toutes les capsules actives en boucle (sin).
    private IEnumerator BlinkCoroutine()
    {
        float time = 0f;

        while (true)
        {
            time += Time.deltaTime;
            // Sin oscillation entre 0 et 1 → remappe vers [_blinkAlphaMin, _blinkAlphaMax].
            float t     = (Mathf.Sin(time * (2f * Mathf.PI / _blinkPeriod)) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(_blinkAlphaMin, _blinkAlphaMax, t);

            ApplyAlphaToActiveCapsules(alpha);
            yield return null;
        }
    }

    // Applique l'alpha donné à toutes les capsules actuellement actives.
    private void ApplyAlphaToActiveCapsules(float alpha)
    {
        for (int i = 0; i < _rowRenderers.Length; i++)
        {
            if (_rowRenderers[i] != null && _rowCapsules[i] != null && _rowCapsules[i].activeSelf)
            {
                Color c = _rowRenderers[i].color;
                c.a = alpha;
                _rowRenderers[i].color = c;
            }
        }

        for (int i = 0; i < _columnRenderers.Length; i++)
        {
            if (_columnRenderers[i] != null && _columnCapsules[i] != null && _columnCapsules[i].activeSelf)
            {
                Color c = _columnRenderers[i].color;
                c.a = alpha;
                _columnRenderers[i].color = c;
            }
        }
    }

    // Arrête la coroutine de blink et remet tous les renderers à alpha 1.
    private void StopBlink()
    {
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }

        ApplyAlphaToActiveCapsules(1f);
    }

    // -------------------------------------------------------------------------
    // Création d'un sprite plein
    // -------------------------------------------------------------------------

    // Crée un GameObject avec le sprite laser assigné et retourne son SpriteRenderer.
    private GameObject CreateFilledSprite(string objectName, Transform parent, out SpriteRenderer sr)
    {
        GameObject go = new GameObject(objectName);
        go.transform.SetParent(parent, false);

        sr              = go.AddComponent<SpriteRenderer>();
        sr.sprite       = _laserSprite;
        sr.sortingOrder = 2;

        return go;
    }
}
