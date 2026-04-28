using UnityEngine;

public class CoinRenderer : MonoBehaviour
{
    [SerializeField] private TurnManager _turnManager;
    [SerializeField] private CoinSystem _coinSystem;
    [SerializeField] private Transform _coinVisual;
    [SerializeField] private SpriteRenderer _coinSpriteRenderer;
    [SerializeField] private Sprite _coinSprite;
    [SerializeField] private float _cellSize = 0.9f;
    [SerializeField] private float _cellGap = 0.1f;
    [SerializeField] private int _gridColumns = 5;
    [SerializeField] private float _gridOffsetY = -1.5f;
    [SerializeField] private float _coinSize = 0.6f;
    [SerializeField] private Color _coinColor = new Color(1f, 0.843f, 0f, 1f);

    private void Awake() // Valide les refs, crée le sprite et affiche la pièce
    {
        if (_turnManager == null)
            throw new MissingReferenceException($"[CoinRenderer] {nameof(_turnManager)} non assigné.");

        if (_coinSystem == null)
            throw new MissingReferenceException($"[CoinRenderer] {nameof(_coinSystem)} non assigné.");

        if (_coinVisual == null)
            throw new MissingReferenceException($"[CoinRenderer] {nameof(_coinVisual)} non assigné.");

        if (_coinSpriteRenderer == null)
            _coinSpriteRenderer = _coinVisual.GetComponent<SpriteRenderer>();

        if (_coinSpriteRenderer == null)
            _coinSpriteRenderer = _coinVisual.gameObject.AddComponent<SpriteRenderer>();

        if (_coinSprite != null)
        {
            _coinSpriteRenderer.sprite = _coinSprite;
            _coinSpriteRenderer.color  = Color.white;
        }
        else // Génère un sprite procédural si aucun n'est assigné
        {
            _coinSpriteRenderer.sprite = CreateFlatSprite();
            _coinSpriteRenderer.color  = _coinColor;
        }

        _coinSpriteRenderer.sortingOrder = 2;
        _coinSpriteRenderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
        _coinVisual.localScale = new Vector3(_coinSize, _coinSize, 1f);

        RefreshDisplay();
    }

    private void OnEnable() // Abonne les écouteurs à l'activation
    {
        if (_turnManager != null)
            _turnManager.OnTurnProcessed.AddListener(HandleTurnProcessed);

        if (_coinSystem != null)
            _coinSystem.OnCoinCollected.AddListener(HandleCoinCollected);
    }

    private void OnDisable() // Désabonne les écouteurs à la désactivation
    {
        if (_turnManager != null)
            _turnManager.OnTurnProcessed.RemoveListener(HandleTurnProcessed);

        if (_coinSystem != null)
            _coinSystem.OnCoinCollected.RemoveListener(HandleCoinCollected);
    }

    private void HandleTurnProcessed() // Rafraîchit l'affichage à la fin du tour
    {
        RefreshDisplay();
    }

    private void HandleCoinCollected(int totalCollected) // Masque la pièce immédiatement après collecte
    {
        RefreshDisplay();
    }

    private void RefreshDisplay() // Met à jour la position et la visibilité de la pièce
    {
        Vector2Int? coinPos = _coinSystem.GetCurrentCoinPos();

        if (coinPos == null) { SetVisible(false); return; } // Masque si aucune pièce active

        _coinVisual.position = GridToWorld(coinPos.Value);
        SetVisible(true);
    }

    private void SetVisible(bool visible) // Active ou désactive le SpriteRenderer
    {
        if (_coinSpriteRenderer != null)
            _coinSpriteRenderer.enabled = visible;
    }

    private Vector3 GridToWorld(Vector2Int gridPos) // Convertit position grille en position monde
    {
        float step = _cellSize + _cellGap;
        float gridOrigin = -((_gridColumns - 1) / 2f) * step;
        float x = gridOrigin + gridPos.x * step;
        float y = gridOrigin + gridPos.y * step + _gridOffsetY;
        return new Vector3(x, y, 0f);
    }

    private Sprite CreateFlatSprite() // Génère un sprite d'un pixel de la couleur de la pièce
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, _coinColor);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
