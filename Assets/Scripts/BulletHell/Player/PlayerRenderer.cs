using System.Collections;
using UnityEngine;

// Initialise le renderer après la grille et le turn manager.
[DefaultExecutionOrder(10)]
// Traduit la position grille du joueur en position monde.
public class PlayerRenderer : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Références
    // -------------------------------------------------------------------------

    // Référence au gestionnaire de grille pour s'abonner.
    [SerializeField] private GridManager _gridManager;

    // Transform du sprite visuel représentant le joueur.
    [SerializeField] private Transform _playerVisual;

    // -------------------------------------------------------------------------
    // Paramètres de mise en page de la grille
    // -------------------------------------------------------------------------

    // Taille d'une cellule, doit correspondre à GridVisualBuilder.
    [SerializeField] private float _cellSize = 0.9f;

    // Espacement entre cellules, doit correspondre à GridVisualBuilder.
    [SerializeField] private float _cellGap = 0.1f;

    // Nombre de colonnes pour le centrage de la grille.
    [SerializeField] private int _gridColumns = 5;

    // Taille du sprite joueur relative à la cellule.
    [SerializeField] private float _playerSize = 0.7f;

    // Décalage vertical de la grille par rapport à l'origine monde.
    [SerializeField] private float _gridOffsetY = -1.5f;

    // Durée du flash jaune après chaque déplacement en secondes.
    [SerializeField] private float _flashDuration = 0.1f;

    // Couleur du joueur au repos, blanc opaque.
    [SerializeField] private Color _playerColor = Color.white;

    // Couleur du flash après déplacement, jaune vif.
    [SerializeField] private Color _flashColor = new Color(1f, 0.843f, 0f, 1f);

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    // SpriteRenderer du visuel joueur pour les changements de couleur.
    private SpriteRenderer _spriteRenderer;

    // Valide les références, crée le sprite et initialise la position.
    private void Awake()
    {
        // Lève une exception si GridManager n'est pas assigné.
        if (_gridManager == null)
        {
            throw new MissingReferenceException(
                $"[PlayerRenderer] La référence {nameof(_gridManager)} n'est pas assignée."
            );
        }

        // Lève une exception si le visuel joueur n'est pas assigné.
        if (_playerVisual == null)
        {
            throw new MissingReferenceException(
                $"[PlayerRenderer] La référence {nameof(_playerVisual)} n'est pas assignée."
            );
        }

        // Récupère ou crée le SpriteRenderer sur le visuel joueur.
        _spriteRenderer = _playerVisual.GetComponent<SpriteRenderer>();

        // Ajoute un SpriteRenderer si le GameObject n'en possède pas.
        if (_spriteRenderer == null)
            _spriteRenderer = _playerVisual.gameObject.AddComponent<SpriteRenderer>();

        // Génère et assigne un sprite blanc plein au joueur.
        _spriteRenderer.sprite        = CreateFlatSprite();
        _spriteRenderer.color         = _playerColor;
        _spriteRenderer.sortingOrder  = 2;

        // Crée un matériau non-éclairé URP pour garantir la visibilité.
        _spriteRenderer.sharedMaterial = new Material(
            Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
        );

        // Applique la taille du joueur via le Transform du visuel.
        _playerVisual.localScale = new Vector3(_playerSize, _playerSize, 1f);

        // Positionne le joueur immédiatement sur sa cellule de départ.
        ApplyWorldPosition(_gridManager.GetPlayerPosition());
    }

    // Abonne l'écouteur de déplacement quand le composant s'active.
    private void OnEnable()
    {
        // Confirme l'abonnement à OnPlayerMoved dans PlayerRenderer
        Debug.Log("[DIAG] PlayerRenderer.OnEnable() — abonnement OnPlayerMoved");
        // Vérifie que la référence est prête avant l'abonnement.
        if (_gridManager != null)
            _gridManager.OnPlayerMoved.AddListener(HandlePlayerMoved);
    }

    // Désabonne l'écouteur de déplacement quand le composant se désactive.
    private void OnDisable()
    {
        // Retire l'abonnement pour éviter des appels sur un objet inactif.
        if (_gridManager != null)
            _gridManager.OnPlayerMoved.RemoveListener(HandlePlayerMoved);
    }

    // -------------------------------------------------------------------------
    // Gestionnaire d'événement
    // -------------------------------------------------------------------------

    // Reçoit la nouvelle position grille et met à jour le visuel.
    private void HandlePlayerMoved(Vector2Int newGridPos)
    {
        // Confirme que PlayerRenderer reçoit bien l'événement joueur
        Debug.Log($"[DIAG] PlayerRenderer.HandlePlayerMoved() reçu — newGridPos={newGridPos}");
        // Déplace immédiatement le sprite à la nouvelle position monde.
        ApplyWorldPosition(newGridPos);

        // Lance le flash jaune pour signaler le déplacement au joueur.
        StartCoroutine(FlashCoroutine());
    }

    // -------------------------------------------------------------------------
    // Coroutine visuelle
    // -------------------------------------------------------------------------

    // Passe la couleur en jaune puis restaure la couleur de repos.
    private IEnumerator FlashCoroutine()
    {
        // Applique immédiatement la couleur de flash jaune.
        _spriteRenderer.color = _flashColor;

        // Attend la durée configurée avant de restaurer la couleur.
        yield return new WaitForSeconds(_flashDuration);

        // Restaure la couleur de repos après le flash.
        _spriteRenderer.color = _playerColor;
    }

    // -------------------------------------------------------------------------
    // Méthodes utilitaires
    // -------------------------------------------------------------------------

    // Calcule la position monde et la transmet au Transform visuel.
    private void ApplyWorldPosition(Vector2Int gridPos)
    {
        // Affiche la position monde calculée pour le joueur
        Debug.Log($"[DIAG] PlayerRenderer.ApplyWorldPosition() — gridPos={gridPos} → worldPos={GridToWorld(gridPos)}");
        // Calcule la position monde à partir de la position de grille.
        _playerVisual.position = GridToWorld(gridPos);
    }

    // Convertit une position de grille en position monde Unity.
    private Vector3 GridToWorld(Vector2Int gridPos)
    {
        // Calcule le pas total incluant la taille et l'espacement.
        float step = _cellSize + _cellGap;

        // Centre la grille autour de l'origine comme GridVisualBuilder.
        float gridOrigin = -((_gridColumns - 1) / 2f) * step;

        // Calcule la coordonnée X à partir de la colonne de grille.
        float x = gridOrigin + gridPos.x * step;

        // Calcule la coordonnée Y à partir de la ligne de grille + offset de descente.
        float y = gridOrigin + gridPos.y * step + _gridOffsetY;

        return new Vector3(x, y, 0f);
    }

    // Génère un sprite blanc d'un pixel pour le visuel joueur.
    private Sprite CreateFlatSprite()
    {
        // Crée une texture minimale d'un pixel de côté.
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
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
