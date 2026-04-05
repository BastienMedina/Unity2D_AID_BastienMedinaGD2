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

    // Couleur rouge appliquée aux indicateurs du prochain laser.
    [SerializeField] private Color _indicatorColor = new Color(1f, 0.133f, 0f, 1f);

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

    // Applique un premier rendu au démarrage avant le premier tour.
    private void Start()
    {
        // Rafraîchit les indicateurs avec le prochain pattern initial.
        RefreshDisplay();
    }

    // Abonne la mise à jour des indicateurs quand le composant s'active.
    private void OnEnable()
    {
        // Vérifie la référence avant d'abonner la mise à jour indicateurs.
        if (_turnManager != null)
            _turnManager.OnTurnProcessed.AddListener(HandleTurnProcessed);
    }

    // Désabonne proprement quand le composant se désactive.
    private void OnDisable()
    {
        // Retire l'abonnement pour éviter les mises à jour fantômes.
        if (_turnManager != null)
            _turnManager.OnTurnProcessed.RemoveListener(HandleTurnProcessed);
    }

    // -------------------------------------------------------------------------
    // Gestionnaire d'événement
    // -------------------------------------------------------------------------

    // Reçoit le signal de fin de tour et rafraîchit les indicateurs.
    private void HandleTurnProcessed()
    {
        // Met à jour les indicateurs selon le prochain pattern laser.
        RefreshDisplay();
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
        for (int row = 0; row < _gridRows; row++)
        {
            // Instancie le cercle et le place sous le conteneur parent.
            GameObject indicator = CreateCircleSprite($"Indicator_Row_{row}", _container);

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
        for (int col = 0; col < _gridColumns; col++)
        {
            // Instancie le cercle et le place sous le conteneur parent.
            GameObject indicator = CreateCircleSprite($"Indicator_Col_{col}", _container);

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
                // Rend l'indicateur de ligne visible pour prévenir le joueur.
                if (_rowIndicators[row] != null)
                    _rowIndicators[row].SetActive(true);
            }
        }

        // Active les indicateurs pour chaque colonne concernée du prochain tour.
        for (int col = 0; col < _gridColumns; col++)
        {
            // Active l'indicateur si toute la colonne sera laser au prochain tour.
            if (nextCols.Contains(col) && IsFullColumn(nextCells, col))
            {
                // Rend l'indicateur de colonne visible pour prévenir le joueur.
                if (_columnIndicators[col] != null)
                    _columnIndicators[col].SetActive(true);
            }
        }
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

    // Crée un GameObject avec un SpriteRenderer carré coloré.
    private GameObject CreateCircleSprite(string objectName, Transform parent)
    {
        // Instancie un GameObject vide et le parent sous le conteneur.
        GameObject go = new GameObject(objectName);
        go.transform.SetParent(parent, false);

        // Crée une texture unie d'un pixel à la couleur indicateur configurée.
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, _indicatorColor);
        tex.Apply();

        // Crée un sprite depuis la texture générée d'un pixel.
        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1f
        );

        // Ajoute un SpriteRenderer et assigne le sprite généré.
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite         = sprite;
        sr.color          = Color.white;
        sr.sortingOrder   = 3;

        return go;
    }
}
