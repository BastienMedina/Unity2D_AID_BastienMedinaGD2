using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Affiche les indicateurs du pattern laser du prochain tour.
public class LaserIndicatorRenderer : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Références
    // -------------------------------------------------------------------------

    // Référence au gestionnaire de tour pour s'abonner.
    [SerializeField] private TurnManager _turnManager;

    // Référence au système laser pour lire le prochain pattern.
    [SerializeField] private LaserSystem _laserSystem;

    // Conteneur parent des sprites indicateurs.
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

    // Nombre de colonnes de la grille pour le calcul des indicateurs.
    [SerializeField] private int _gridColumns = 5;

    // Nombre de lignes de la grille pour le calcul des indicateurs.
    [SerializeField] private int _gridRows = 5;

    // Taille du cercle indicateur en unités monde.
    [SerializeField] private float _indicatorSize = 0.4f;

    // Décalage depuis le bord de la grille pour placer l'indicateur.
    [SerializeField] private float _indicatorOffset = 0.8f;

    // Sprite utilisé pour les cercles indicateurs — doit être assigné dans l'Inspector.
    [SerializeField] private Sprite _indicatorSprite;

    // -------------------------------------------------------------------------
    // Paramètres de feedback — scale punch
    // -------------------------------------------------------------------------

    // Durée du punch de scale à l'apparition d'un indicateur (secondes).
    [SerializeField] private float _punchDuration = 0.25f;

    // Amplitude du punch (valeur multipliée à _indicatorSize).
    [SerializeField] private float _punchScale = 1.5f;

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    // Cercles indicateurs pour chaque ligne, indexés par numéro de ligne.
    private GameObject[] _rowIndicators;

    // Cercles indicateurs pour chaque colonne, indexés par numéro de colonne.
    private GameObject[] _columnIndicators;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Valide les références et crée les indicateurs au démarrage.
    private void Awake()
    {
        // Lève une exception si TurnManager n'est pas assigné.
        if (_turnManager == null)
        {
            throw new MissingReferenceException(
                $"[LaserIndicatorRenderer] La référence {nameof(_turnManager)} n'est pas assignée."
            );
        }

        // Lève une exception si LaserSystem n'est pas assigné.
        if (_laserSystem == null)
        {
            throw new MissingReferenceException(
                $"[LaserIndicatorRenderer] La référence {nameof(_laserSystem)} n'est pas assignée."
            );
        }

        // Crée les cercles indicateurs pour toutes les lignes et colonnes.
        BuildIndicators();
    }

    // Applique un premier rendu et abonne la mise à jour après Awake.
    private void Start()
    {
        _turnManager.OnTurnProcessed.AddListener(RefreshDisplay);
        RefreshDisplay();
    }

    // Désabonne proprement quand le composant se désactive.
    private void OnDisable()
    {
        if (_turnManager != null)
            _turnManager.OnTurnProcessed.RemoveListener(RefreshDisplay);
    }

    // -------------------------------------------------------------------------
    // Construction des indicateurs
    // -------------------------------------------------------------------------

    // Instancie un cercle indicateur par ligne et par colonne.
    private void BuildIndicators()
    {
        // Alloue les tableaux pour les indicateurs de lignes et colonnes.
        _rowIndicators    = new GameObject[_gridRows];
        _columnIndicators = new GameObject[_gridColumns];

        // Calcule le pas total cellule + espacement une seule fois.
        float step = _cellSize + _cellGap;

        // Calcule la position X du bord gauche de la grille.
        float leftEdge = _gridOrigin.x - _indicatorOffset;

        // Crée un indicateur circulaire pour chaque ligne de la grille.
        // Rotation 270° : la pointe (bas du sprite) pointe vers la gauche, l'arrondi (haut) vers la droite.
        for (int row = 0; row < _gridRows; row++)
        {
            // Instancie le sprite orienté pour tirer vers la gauche depuis le bord gauche.
            GameObject indicator = CreateCircleSprite($"Indicator_Row_{row}", _container, 270f);

            // Calcule la coordonnée Y centrée sur la ligne courante.
            float y = _gridOrigin.y + row * step;

            // Positionne l'indicateur à gauche de la grille sur cette ligne.
            indicator.transform.position   = new Vector3(leftEdge, y, 0f);
            indicator.transform.localScale = Vector3.one * _indicatorSize;

            // Cache l'indicateur jusqu'à ce qu'il soit pertinent.
            indicator.SetActive(false);

            // Stocke l'indicateur à l'index correspondant à sa ligne.
            _rowIndicators[row] = indicator;
        }

        // Calcule la position Y du bord supérieur de la grille.
        float topEdge = _gridOrigin.y + (_gridRows - 1) * step + _indicatorOffset;

        // Crée un indicateur circulaire pour chaque colonne de la grille.
        // Rotation 180° : la pointe (bas du sprite) pointe vers le bas, l'arrondi (haut) reste en haut.
        for (int col = 0; col < _gridColumns; col++)
        {
            // Instancie le sprite orienté pour tirer vers le bas depuis le bord supérieur.
            GameObject indicator = CreateCircleSprite($"Indicator_Col_{col}", _container, 180f);

            // Calcule la coordonnée X centrée sur la colonne courante.
            float x = _gridOrigin.x + col * step;

            // Positionne l'indicateur au-dessus de la grille sur cette colonne.
            indicator.transform.position   = new Vector3(x, topEdge, 0f);
            indicator.transform.localScale = Vector3.one * _indicatorSize;

            // Cache l'indicateur jusqu'à ce qu'il soit pertinent.
            indicator.SetActive(false);

            // Stocke l'indicateur à l'index correspondant à sa colonne.
            _columnIndicators[col] = indicator;
        }
    }

    // -------------------------------------------------------------------------
    // Rafraîchissement de l'affichage
    // -------------------------------------------------------------------------

    // Cache tous les indicateurs puis active ceux du prochain pattern.
    private void RefreshDisplay()
    {
        // Cache tous les indicateurs de lignes avant la mise à jour.
        for (int row = 0; row < _rowIndicators.Length; row++)
        {
            // Désactive l'indicateur de ligne s'il est valide.
            if (_rowIndicators[row] != null)
                _rowIndicators[row].SetActive(false);
        }

        // Cache tous les indicateurs de colonnes avant la mise à jour.
        for (int col = 0; col < _columnIndicators.Length; col++)
        {
            // Désactive l'indicateur de colonne s'il est valide.
            if (_columnIndicators[col] != null)
                _columnIndicators[col].SetActive(false);
        }

        // Récupère les cellules du prochain pattern laser à venir.
        List<Vector2Int> nextCells = _laserSystem.GetNextLaserCells();

        // Identifie les lignes et colonnes couvertes par le prochain pattern.
        HashSet<int> nextRows = new HashSet<int>();
        HashSet<int> nextCols = new HashSet<int>();

        // Parcourt les cellules pour cataloguer les lignes et colonnes actives.
        for (int i = 0; i < nextCells.Count; i++)
        {
            // Enregistre le numéro de ligne de la cellule courante.
            nextRows.Add(nextCells[i].y);

            // Enregistre le numéro de colonne de la cellule courante.
            nextCols.Add(nextCells[i].x);
        }

        // Active les indicateurs pour chaque ligne concernée du prochain tour.
        for (int row = 0; row < _gridRows; row++)
        {
            // Active l'indicateur si toute la ligne sera laser au prochain tour.
            if (nextRows.Contains(row) && IsFullRow(nextCells, row))
            {
                if (_rowIndicators[row] != null)
                {
                    _rowIndicators[row].SetActive(true);
                    StartCoroutine(ScalePunchCoroutine(_rowIndicators[row].transform));
                }
            }
        }

        // Active les indicateurs pour chaque colonne concernée du prochain tour.
        for (int col = 0; col < _gridColumns; col++)
        {
            // Active l'indicateur si toute la colonne sera laser au prochain tour.
            if (nextCols.Contains(col) && IsFullColumn(nextCells, col))
            {
                if (_columnIndicators[col] != null)
                {
                    _columnIndicators[col].SetActive(true);
                    StartCoroutine(ScalePunchCoroutine(_columnIndicators[col].transform));
                }
            }
        }
    }

    // -------------------------------------------------------------------------
    // Feedback — scale punch
    // -------------------------------------------------------------------------

    // Gonfle l'indicateur jusqu'à _punchScale * _indicatorSize puis revient à la normale.
    private IEnumerator ScalePunchCoroutine(Transform indicator)
    {
        float elapsed   = 0f;
        float half      = _punchDuration * 0.5f;
        float baseSize  = _indicatorSize;
        float peakSize  = _indicatorSize * _punchScale;

        // Phase montante
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.Clamp01(elapsed / half);
            float s  = Mathf.Lerp(baseSize, peakSize, t);
            indicator.localScale = Vector3.one * s;
            yield return null;
        }

        // Phase descendante
        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.Clamp01(elapsed / half);
            float s  = Mathf.Lerp(peakSize, baseSize, t);
            indicator.localScale = Vector3.one * s;
            yield return null;
        }

        indicator.localScale = Vector3.one * baseSize;
    }

    // Vérifie si toutes les cellules d'une ligne sont dans le prochain pattern.
    private bool IsFullRow(List<Vector2Int> cells, int row)
    {
        // Compte les cellules du prochain pattern sur cette ligne.
        int count = 0;

        // Parcourt les cellules pour compter celles sur cette ligne.
        for (int i = 0; i < cells.Count; i++)
        {
            // Incrémente le compteur si la cellule appartient à cette ligne.
            if (cells[i].y == row)
                count++;
        }

        // Retourne vrai si toutes les colonnes de la ligne sont couvertes.
        return count >= _gridColumns;
    }

    // Vérifie si toutes les cellules d'une colonne sont dans le prochain pattern.
    private bool IsFullColumn(List<Vector2Int> cells, int col)
    {
        // Compte les cellules du prochain pattern sur cette colonne.
        int count = 0;

        // Parcourt les cellules pour compter celles sur cette colonne.
        for (int i = 0; i < cells.Count; i++)
        {
            // Incrémente le compteur si la cellule appartient à cette colonne.
            if (cells[i].x == col)
                count++;
        }

        // Retourne vrai si toutes les lignes de la colonne sont couvertes.
        return count >= _gridRows;
    }

    // -------------------------------------------------------------------------
    // Méthode utilitaire
    // -------------------------------------------------------------------------

    // Crée un GameObject avec le sprite indicateur assigné et la rotation donnée.
    private GameObject CreateCircleSprite(string objectName, Transform parent, float rotationZ = 0f)
    {
        GameObject go = new GameObject(objectName);
        go.transform.SetParent(parent, false);

        // Applique la rotation autour de Z pour orienter la pointe correctement.
        go.transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = _indicatorSprite;
        sr.sortingOrder = 3;

        return go;
    }
}
