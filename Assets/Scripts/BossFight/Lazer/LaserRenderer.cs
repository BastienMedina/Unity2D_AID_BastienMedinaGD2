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
    // État interne
    // -------------------------------------------------------------------------

    // Capsule pour chaque ligne, indexée par numéro de ligne.
    private GameObject[] _rowCapsules;

    // Capsule pour chaque colonne, indexée par numéro de colonne.
    private GameObject[] _columnCapsules;

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
    }

    // -------------------------------------------------------------------------
    // Construction du pool de capsules
    // -------------------------------------------------------------------------

    // Instancie une capsule pleine par ligne et par colonne, toutes cachées.
    private void BuildCapsules()
    {
        _rowCapsules    = new GameObject[_gridRows];
        _columnCapsules = new GameObject[_gridColumns];

        float step       = _cellSize + _cellGap;
        float fullWidth  = _gridColumns * _cellSize + (_gridColumns - 1) * _cellGap;
        float fullHeight = _gridRows    * _cellSize + (_gridRows    - 1) * _cellGap;

        // Taille monde native du sprite (pixels / PPU) — base pour le calcul d'échelle.
        float spriteW = _laserSprite.bounds.size.x;
        float spriteH = _laserSprite.bounds.size.y;

        // Capsule horizontale — sprite déjà orienté en largeur, pas de rotation.
        for (int row = 0; row < _gridRows; row++)
        {
            GameObject capsule = CreateFilledSprite($"Laser_Row_{row}", _container);

            float x = _gridOrigin.x + (_gridColumns - 1) * step * 0.5f;
            float y = _gridOrigin.y + row * step;

            capsule.transform.position   = new Vector3(x, y, 0f);
            // Divise la taille voulue par la taille native du sprite pour obtenir le bon scale.
            capsule.transform.localScale = new Vector3(fullWidth / spriteW, _cellSize / spriteH, 1f);
            capsule.SetActive(false);

            _rowCapsules[row] = capsule;
        }

        // Capsule verticale — rotation 90° pour orienter le sprite horizontal en vertical.
        for (int col = 0; col < _gridColumns; col++)
        {
            GameObject capsule = CreateFilledSprite($"Laser_Col_{col}", _container);

            float x = _gridOrigin.x + col * step;
            float y = _gridOrigin.y + (_gridRows - 1) * step * 0.5f;

            capsule.transform.position = new Vector3(x, y, 0f);
            capsule.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            // Après rotation 90° : localScale.x → hauteur monde, localScale.y → largeur monde.
            capsule.transform.localScale = new Vector3(fullHeight / spriteW, _cellSize / spriteH, 1f);
            capsule.SetActive(false);

            _columnCapsules[col] = capsule;
        }
    }

    // -------------------------------------------------------------------------
    // Rafraîchissement de l'affichage
    // -------------------------------------------------------------------------

    // Cache toutes les capsules puis active celles correspondant au pattern courant.
    private void RefreshDisplay()
    {
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
    // Création d'un sprite plein
    // -------------------------------------------------------------------------

    // Crée un GameObject avec le sprite laser assigné.
    private GameObject CreateFilledSprite(string objectName, Transform parent)
    {
        GameObject go = new GameObject(objectName);
        go.transform.SetParent(parent, false);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = _laserSprite;
        sr.sortingOrder = 2;

        return go;
    }
}
