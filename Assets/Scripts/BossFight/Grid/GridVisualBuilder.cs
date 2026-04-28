using UnityEngine;

public class GridVisualBuilder : MonoBehaviour
{
    [SerializeField] private int _gridColumns = 5;
    [SerializeField] private int _gridRows = 5;
    [SerializeField] private float _cellSize = 0.9f;
    [SerializeField] private float _cellGap = 0.1f;
    [SerializeField] private float _gridOffsetY = -1.5f;
    [SerializeField] private Sprite _cellSprite;

    private void Awake() // Détruit les anciennes cellules et reconstruit la grille
    {
        DestroyExistingCells();
        BuildGrid();
    }

    private void DestroyExistingCells() // Supprime tous les enfants existants
    {
        for (int i = transform.childCount - 1; i >= 0; i--) // Parcourt en sens inverse
            Destroy(transform.GetChild(i).gameObject);
    }

    private void BuildGrid() // Instancie et positionne chaque cellule
    {
        float step   = _cellSize + _cellGap;
        float origin = -((_gridColumns - 1) / 2f) * step; // Centre la grille autour de l'origine

        for (int col = 0; col < _gridColumns; col++) // Parcourt chaque colonne
        {
            for (int row = 0; row < _gridRows; row++) // Parcourt chaque ligne
            {
                float x = origin + col * step;
                float y = origin + row * step + _gridOffsetY;
                CreateCell(col, row, new Vector3(x, y, 0f));
            }
        }
    }

    private void CreateCell(int col, int row, Vector3 worldPos) // Crée un sprite de cellule à la position
    {
        GameObject cell = new GameObject($"Cell_{col}_{row}");
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = worldPos;
        cell.transform.localScale    = new Vector3(_cellSize, _cellSize, 1f);

        SpriteRenderer sr = cell.AddComponent<SpriteRenderer>();
        sr.sprite       = _cellSprite;
        sr.sortingOrder = 0;
    }
}
