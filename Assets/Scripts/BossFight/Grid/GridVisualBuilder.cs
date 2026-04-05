using UnityEngine;

// Construit et positionne les 25 cellules visuelles de la grille.
public class GridVisualBuilder : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Paramètres de la grille
    // -------------------------------------------------------------------------

    // Nombre de colonnes de la grille à afficher.
    [SerializeField] private int _gridColumns = 5;

    // Nombre de lignes de la grille à afficher.
    [SerializeField] private int _gridRows = 5;

    // Taille d'une cellule en unités monde Unity.
    [SerializeField] private float _cellSize = 0.9f;

    // Espacement entre deux cellules adjacentes.
    [SerializeField] private float _cellGap = 0.1f;

    // -------------------------------------------------------------------------
    // Paramètres visuels
    // -------------------------------------------------------------------------

    // Couleur blanche semi-transparente des cellules de fond.
    [SerializeField] private Color _cellColor = new Color(1f, 1f, 1f, 0.15f);

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Construit toutes les cellules visuelles au démarrage du composant.
    private void Awake()
    {
        // Détruit les cellules précédentes si Awake est rappelé.
        DestroyExistingCells();

        // Génère chaque cellule aux bonnes coordonnées monde.
        BuildGrid();
    }

    // -------------------------------------------------------------------------
    // Méthodes privées
    // -------------------------------------------------------------------------

    // Supprime les enfants existants avant de reconstruire la grille.
    private void DestroyExistingCells()
    {
        // Parcourt tous les enfants du conteneur et les détruit.
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            // Détruit immédiatement sans délai car on est hors Play.
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    // Instancie et positionne chaque cellule de la grille.
    private void BuildGrid()
    {
        // Calcule le pas total entre deux centres de cellule adjacentes.
        float step = _cellSize + _cellGap;

        // Calcule l'origine pour centrer la grille autour de (0,0).
        float origin = -(((_gridColumns - 1) / 2f) * step);

        // Crée le sprite partagé par toutes les cellules de la grille.
        Sprite cellSprite = CreateFlatSprite(_cellColor);

        // Parcourt chaque colonne de la grille à construire.
        for (int col = 0; col < _gridColumns; col++)
        {
            // Parcourt chaque ligne de la colonne courante.
            for (int row = 0; row < _gridRows; row++)
            {
                // Calcule la position monde centrée de la cellule courante.
                float x = origin + col * step;
                float y = origin + row * step;

                // Instancie la cellule visuelle et la parent sous ce transform.
                CreateCell(col, row, new Vector3(x, y, 0f), cellSprite);
            }
        }
    }

    // Crée une cellule visuelle à la position monde donnée.
    private void CreateCell(int col, int row, Vector3 worldPos, Sprite sprite)
    {
        // Crée un GameObject vide nommé d'après ses coordonnées de grille.
        GameObject cell = new GameObject($"Cell_{col}_{row}");
        cell.transform.SetParent(transform, false);

        // Positionne la cellule à sa coordonnée monde calculée.
        cell.transform.position   = worldPos;

        // Redimensionne la cellule selon la taille configurée.
        cell.transform.localScale = new Vector3(_cellSize, _cellSize, 1f);

        // Ajoute un SpriteRenderer et assigne le sprite plat généré.
        SpriteRenderer sr   = cell.AddComponent<SpriteRenderer>();
        sr.sprite           = sprite;
        sr.color            = Color.white;
        sr.sortingOrder     = 0;
    }

    // Génère un sprite d'un pixel à la couleur unie donnée.
    private Sprite CreateFlatSprite(Color color)
    {
        // Crée une texture minimale d'un pixel de côté.
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();

        // Crée et retourne le sprite depuis la texture générée.
        return Sprite.Create(
            tex,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1f
        );
    }
}
