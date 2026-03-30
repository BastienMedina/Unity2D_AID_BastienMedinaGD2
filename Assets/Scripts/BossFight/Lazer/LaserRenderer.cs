using System.Collections.Generic;
using UnityEngine;

// Affiche les cellules laser actives du pattern courant.
public class LaserRenderer : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Références
    // -------------------------------------------------------------------------

    // Référence au gestionnaire de tour pour s'abonner.
    [SerializeField] private TurnManager _turnManager;

    // Référence au système laser pour lire les cellules actives.
    [SerializeField] private LaserSystem _laserSystem;

    // Conteneur parent des sprites capsule laser.
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

    // Couleur orange appliquée aux capsules laser actives.
    [SerializeField] private Color _laserColor = new Color(1f, 0.549f, 0f, 1f);

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    // Capsules pour chaque ligne, indexées par numéro de ligne.
    private GameObject[] _rowCapsules;

    // Capsules pour chaque colonne, indexées par numéro de colonne.
    private GameObject[] _columnCapsules;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Valide les références et crée les capsules laser au démarrage.
    private void Awake()
    {
        // Lève une exception si TurnManager n'est pas assigné.
        if (_turnManager == null)
        {
            throw new MissingReferenceException(
                $"[LaserRenderer] La référence {nameof(_turnManager)} n'est pas assignée."
            );
        }

        // Lève une exception si LaserSystem n'est pas assigné.
        if (_laserSystem == null)
        {
            throw new MissingReferenceException(
                $"[LaserRenderer] La référence {nameof(_laserSystem)} n'est pas assignée."
            );
        }

        // Crée les capsules pour toutes les lignes et colonnes.
        BuildCapsules();
    }

    // Applique un premier rendu au démarrage avant le premier tour.
    private void Start()
    {
        // Rafraîchit l'affichage avec le pattern laser initial.
        RefreshDisplay();
    }

    // Abonne la mise à jour visuelle quand le composant s'active.
    private void OnEnable()
    {
        // Vérifie la référence avant d'abonner la mise à jour laser.
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

    // Reçoit le signal de fin de tour et rafraîchit l'affichage.
    private void HandleTurnProcessed()
    {
        // Met à jour les capsules selon le pattern laser courant.
        RefreshDisplay();
    }

    // -------------------------------------------------------------------------
    // Construction des capsules
    // -------------------------------------------------------------------------

    // Instancie une capsule par ligne et par colonne de la grille.
    private void BuildCapsules()
    {
        // Alloue les tableaux pour les capsules de lignes et colonnes.
        _rowCapsules    = new GameObject[_gridRows];
        _columnCapsules = new GameObject[_gridColumns];

        // Calcule le pas total cellule + espacement une seule fois.
        float step         = _cellSize + _cellGap;

        // Calcule la longueur totale d'une rangée ou colonne complète.
        float stripLength  = _gridColumns * _cellSize + (_gridColumns - 1) * _cellGap;

        // Crée une capsule horizontale pour chaque ligne de la grille.
        for (int row = 0; row < _gridRows; row++)
        {
            // Instancie le GameObject capsule et le place sous le conteneur.
            GameObject capsule = CreateCapsuleSprite($"Laser_Row_{row}", _container);

            // Calcule la coordonnée Y centrée sur la ligne courante.
            float y = _gridOrigin.y + row * step;

            // Calcule la coordonnée X centrée sur l'ensemble de la rangée.
            float x = _gridOrigin.x + (_gridColumns - 1) * step * 0.5f;

            // Positionne la capsule horizontale au centre de la ligne.
            capsule.transform.position = new Vector3(x, y, 0f);

            // Étire la capsule pour couvrir toute la longueur de la ligne.
            capsule.transform.localScale = new Vector3(stripLength, _cellSize * 0.8f, 1f);

            // Cache la capsule jusqu'à ce que le laser l'active.
            capsule.SetActive(false);

            // Stocke la capsule à l'index correspondant à sa ligne.
            _rowCapsules[row] = capsule;
        }

        // Crée une capsule verticale pour chaque colonne de la grille.
        for (int col = 0; col < _gridColumns; col++)
        {
            // Instancie le GameObject capsule et le place sous le conteneur.
            GameObject capsule = CreateCapsuleSprite($"Laser_Col_{col}", _container);

            // Calcule la coordonnée X centrée sur la colonne courante.
            float x = _gridOrigin.x + col * step;

            // Calcule la coordonnée Y centrée sur l'ensemble de la colonne.
            float y = _gridOrigin.y + (_gridRows - 1) * step * 0.5f;

            // Positionne la capsule verticale au centre de la colonne.
            capsule.transform.position = new Vector3(x, y, 0f);

            // Étire la capsule pour couvrir toute la hauteur de la colonne.
            capsule.transform.localScale = new Vector3(_cellSize * 0.8f, stripLength, 1f);

            // Cache la capsule jusqu'à ce que le laser l'active.
            capsule.SetActive(false);

            // Stocke la capsule à l'index correspondant à sa colonne.
            _columnCapsules[col] = capsule;
        }
    }

    // -------------------------------------------------------------------------
    // Rafraîchissement de l'affichage
    // -------------------------------------------------------------------------

    // Cache toutes les capsules puis active celles du pattern courant.
    private void RefreshDisplay()
    {
        // Cache toutes les capsules de lignes avant la mise à jour.
        for (int row = 0; row < _rowCapsules.Length; row++)
        {
            // Désactive la capsule de ligne si elle est valide.
            if (_rowCapsules[row] != null)
                _rowCapsules[row].SetActive(false);
        }

        // Cache toutes les capsules de colonnes avant la mise à jour.
        for (int col = 0; col < _columnCapsules.Length; col++)
        {
            // Désactive la capsule de colonne si elle est valide.
            if (_columnCapsules[col] != null)
                _columnCapsules[col].SetActive(false);
        }

        // Récupère les cellules actives du pattern laser courant.
        List<Vector2Int> activeCells = _laserSystem.GetCurrentLaserCells();

        // Identifie les lignes et colonnes couvertes par les cellules actives.
        HashSet<int> activeRows    = new HashSet<int>();
        HashSet<int> activeCols    = new HashSet<int>();

        // Parcourt chaque cellule active pour cataloguer lignes et colonnes.
        for (int i = 0; i < activeCells.Count; i++)
        {
            // Enregistre le numéro de ligne de la cellule courante.
            activeRows.Add(activeCells[i].y);

            // Enregistre le numéro de colonne de la cellule courante.
            activeCols.Add(activeCells[i].x);
        }

        // Vérifie si une ligne entière est couverte pour l'afficher.
        for (int row = 0; row < _gridRows; row++)
        {
            // Active la capsule si toute la ligne est dans le pattern.
            if (activeRows.Contains(row) && IsFullRow(activeCells, row))
            {
                // Rend la capsule de ligne visible avec la couleur laser.
                if (_rowCapsules[row] != null)
                    _rowCapsules[row].SetActive(true);
            }
        }

        // Vérifie si une colonne entière est couverte pour l'afficher.
        for (int col = 0; col < _gridColumns; col++)
        {
            // Active la capsule si toute la colonne est dans le pattern.
            if (activeCols.Contains(col) && IsFullColumn(activeCells, col))
            {
                // Rend la capsule de colonne visible avec la couleur laser.
                if (_columnCapsules[col] != null)
                    _columnCapsules[col].SetActive(true);
            }
        }
    }

    // Vérifie si toutes les cellules d'une ligne sont actives.
    private bool IsFullRow(List<Vector2Int> cells, int row)
    {
        // Compte les cellules actives sur la ligne vérifiée.
        int count = 0;

        // Parcourt les cellules pour compter celles sur cette ligne.
        for (int i = 0; i < cells.Count; i++)
        {
            // Incrémente le compteur si la cellule est sur cette ligne.
            if (cells[i].y == row)
                count++;
        }

        // Retourne vrai si toutes les colonnes sont représentées.
        return count >= _gridColumns;
    }

    // Vérifie si toutes les cellules d'une colonne sont actives.
    private bool IsFullColumn(List<Vector2Int> cells, int col)
    {
        // Compte les cellules actives sur la colonne vérifiée.
        int count = 0;

        // Parcourt les cellules pour compter celles sur cette colonne.
        for (int i = 0; i < cells.Count; i++)
        {
            // Incrémente le compteur si la cellule est sur cette colonne.
            if (cells[i].x == col)
                count++;
        }

        // Retourne vrai si toutes les lignes sont représentées.
        return count >= _gridRows;
    }

    // -------------------------------------------------------------------------
    // Méthode utilitaire
    // -------------------------------------------------------------------------

    // Crée un GameObject avec un SpriteRenderer rectangulaire coloré.
    private GameObject CreateCapsuleSprite(string objectName, Transform parent)
    {
        // Instancie un GameObject vide sous le conteneur parent.
        GameObject go = new GameObject(objectName);
        go.transform.SetParent(parent, false);

        // Crée une texture unie d'un pixel à la couleur laser configurée.
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, _laserColor);
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
        sr.sortingOrder   = 2;

        return go;
    }
}
