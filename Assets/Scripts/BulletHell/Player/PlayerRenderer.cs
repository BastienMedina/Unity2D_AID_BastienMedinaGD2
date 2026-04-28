using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(10)]
public class PlayerRenderer : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private Transform _playerVisual;
    [SerializeField] private float _cellSize = 0.9f;
    [SerializeField] private float _cellGap = 0.1f;
    [SerializeField] private int _gridColumns = 5;
    [SerializeField] private float _playerSize = 0.7f;
    [SerializeField] private float _gridOffsetY = -1.5f;
    [SerializeField] private float _flashDuration = 0.1f;
    [SerializeField] private Color _playerColor = Color.white;
    [SerializeField] private Color _flashColor = new Color(1f, 0.843f, 0f, 1f);

    private SpriteRenderer _spriteRenderer;

    private void Awake() // Valide refs, crée sprite et positionne sur la grille
    {
        if (_gridManager == null)
            throw new MissingReferenceException($"[PlayerRenderer] {nameof(_gridManager)} non assigné.");

        if (_playerVisual == null)
            throw new MissingReferenceException($"[PlayerRenderer] {nameof(_playerVisual)} non assigné.");

        _spriteRenderer = _playerVisual.GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
            _spriteRenderer = _playerVisual.gameObject.AddComponent<SpriteRenderer>();

        if (_spriteRenderer.sprite == null) // Crée un sprite blanc de fallback
            _spriteRenderer.sprite = CreateFlatSprite();

        _spriteRenderer.color        = _playerColor;
        _spriteRenderer.sortingOrder = 2;

        if (_spriteRenderer.sharedMaterial == null)
            _spriteRenderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));

        _playerVisual.localScale = new Vector3(_playerSize, _playerSize, 1f);
        ApplyWorldPosition(_gridManager.GetPlayerPosition());
    }

    private void OnEnable() // S'abonne au déplacement joueur
    {
        if (_gridManager != null)
            _gridManager.OnPlayerMoved.AddListener(HandlePlayerMoved);
    }

    private void OnDisable() // Désabonne le listener de déplacement
    {
        if (_gridManager != null)
            _gridManager.OnPlayerMoved.RemoveListener(HandlePlayerMoved);
    }

    private void HandlePlayerMoved(Vector2Int newGridPos) // Déplace le visuel et joue le flash
    {
        ApplyWorldPosition(newGridPos);
        StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine() // Flash jaune puis retour à la couleur de repos
    {
        _spriteRenderer.color = _flashColor;
        yield return new WaitForSeconds(_flashDuration);
        _spriteRenderer.color = _playerColor;
    }

    private void ApplyWorldPosition(Vector2Int gridPos) // Calcule et applique la position monde
    {
        _playerVisual.position = GridToWorld(gridPos);
    }

    private Vector3 GridToWorld(Vector2Int gridPos) // Convertit position grille en position monde
    {
        float step       = _cellSize + _cellGap;
        float gridOrigin = -((_gridColumns - 1) / 2f) * step; // Centre la grille autour de l'origine
        float x          = gridOrigin + gridPos.x * step;
        float y          = gridOrigin + gridPos.y * step + _gridOffsetY;
        return new Vector3(x, y, 0f);
    }

    private Sprite CreateFlatSprite() // Génère un sprite blanc d'un pixel de fallback
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
