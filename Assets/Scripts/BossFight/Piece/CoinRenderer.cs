using UnityEngine;

// Affiche la position et la visibilité de la pièce active.
public class CoinRenderer : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Références
    // -------------------------------------------------------------------------

    // Référence au gestionnaire de tour pour s'abonner.
    [SerializeField] private TurnManager _turnManager;

    // Référence au système de pièces pour lire la position active.
    [SerializeField] private CoinSystem _coinSystem;

    // Transform du sprite visuel représentant la pièce.
    [SerializeField] private Transform _coinVisual;

    // SpriteRenderer de la pièce pour gérer sa visibilité.
    [SerializeField] private SpriteRenderer _coinSpriteRenderer;

    // Sprite personnalisé pour la pièce — si non assigné, utilise un sprite généré.
    [SerializeField] private Sprite _coinSprite;

    // -------------------------------------------------------------------------
    // Paramètres de mise en page de la grille
    // -------------------------------------------------------------------------

    // Taille d'une cellule en unités monde Unity.
    [SerializeField] private float _cellSize = 0.9f;

    // Espacement entre les cellules en unités monde.
    [SerializeField] private float _cellGap = 0.1f;

    // Nombre de colonnes pour le centrage de la grille.
    [SerializeField] private int _gridColumns = 5;

    // Décalage vertical de la grille par rapport à l'origine monde, doit correspondre au GO parent.
    [SerializeField] private float _gridOffsetY = -1.5f;

    // Taille du sprite pièce relative à la cellule.
    [SerializeField] private float _coinSize = 0.6f;

    // Couleur appliquée au sprite de la pièce (ignorée si _coinSprite est assigné).
    [SerializeField] private Color _coinColor = new Color(1f, 0.843f, 0f, 1f);

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Valide les références, crée le sprite et s'abonne aux événements.
    private void Awake()
    {
        // Lève une exception si TurnManager n'est pas assigné.
        if (_turnManager == null)
        {
            throw new MissingReferenceException(
                $"[CoinRenderer] La référence {nameof(_turnManager)} n'est pas assignée."
            );
        }

        // Lève une exception si CoinSystem n'est pas assigné.
        if (_coinSystem == null)
        {
            throw new MissingReferenceException(
                $"[CoinRenderer] La référence {nameof(_coinSystem)} n'est pas assignée."
            );
        }

        // Lève une exception si le visuel de pièce n'est pas assigné.
        if (_coinVisual == null)
        {
            throw new MissingReferenceException(
                $"[CoinRenderer] La référence {nameof(_coinVisual)} n'est pas assignée."
            );
        }

        // Récupère ou crée le SpriteRenderer sur le visuel de la pièce.
        if (_coinSpriteRenderer == null)
            _coinSpriteRenderer = _coinVisual.GetComponent<SpriteRenderer>();

        // Ajoute un SpriteRenderer si le GameObject n'en possède pas.
        if (_coinSpriteRenderer == null)
            _coinSpriteRenderer = _coinVisual.gameObject.AddComponent<SpriteRenderer>();

        // Génère ou assigne le sprite de la pièce.
        if (_coinSprite != null)
        {
            // Utilise le sprite assigné en Inspector.
            _coinSpriteRenderer.sprite = _coinSprite;
            _coinSpriteRenderer.color  = Color.white;
        }
        else
        {
            // Fallback : sprite procédural coloré.
            _coinSpriteRenderer.sprite = CreateFlatSprite();
            _coinSpriteRenderer.color  = _coinColor;
        }
        _coinSpriteRenderer.sortingOrder = 2;

        // Bascule sur le matériau non-éclairé URP pour la visibilité.
        _coinSpriteRenderer.sharedMaterial = new Material(
            Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
        );

        // Applique la taille de la pièce via le Transform du visuel.
        _coinVisual.localScale = new Vector3(_coinSize, _coinSize, 1f);

        // Positionne et affiche la pièce immédiatement au démarrage.
        RefreshDisplay();
    }

    // Abonne les écouteurs quand le composant s'active.
    private void OnEnable()
    {
        // Vérifie que TurnManager est prêt avant l'abonnement.
        if (_turnManager != null)
            _turnManager.OnTurnProcessed.AddListener(HandleTurnProcessed);

        // Vérifie que CoinSystem est prêt avant l'abonnement.
        if (_coinSystem != null)
            _coinSystem.OnCoinCollected.AddListener(HandleCoinCollected);
    }

    // Désabonne les écouteurs quand le composant se désactive.
    private void OnDisable()
    {
        // Retire l'abonnement pour éviter des appels sur objet inactif.
        if (_turnManager != null)
            _turnManager.OnTurnProcessed.RemoveListener(HandleTurnProcessed);

        // Retire l'abonnement pour éviter des fuites mémoire coin.
        if (_coinSystem != null)
            _coinSystem.OnCoinCollected.RemoveListener(HandleCoinCollected);
    }

    // -------------------------------------------------------------------------
    // Gestionnaires d'événements
    // -------------------------------------------------------------------------

    // Reçoit le signal de fin de tour et met à jour l'affichage.
    private void HandleTurnProcessed()
    {
        // Rafraîchit la position et la visibilité de la pièce active.
        RefreshDisplay();
    }

    // Reçoit le signal de collecte et masque immédiatement la pièce.
    private void HandleCoinCollected(int totalCollected)
    {
        // Met à jour l'affichage après la collecte d'une pièce.
        RefreshDisplay();
    }

    // -------------------------------------------------------------------------
    // Rafraîchissement de l'affichage
    // -------------------------------------------------------------------------

    // Lit la position de la pièce et met à jour le sprite visuel.
    private void RefreshDisplay()
    {
        // Récupère la position de la pièce active ou null si absente.
        Vector2Int? coinPos = _coinSystem.GetCurrentCoinPos();

        // Masque le visuel si aucune pièce n'est actuellement active.
        if (coinPos == null)
        {
            // Désactive le SpriteRenderer pour cacher la pièce.
            SetVisible(false);
            return;
        }

        // Calcule la position monde correspondant à la cellule de la pièce.
        Vector3 worldPos = GridToWorld(coinPos.Value);

        // Déplace le sprite vers la position monde calculée.
        _coinVisual.position = worldPos;

        // Rend le visuel visible puisque la pièce est active sur la grille.
        SetVisible(true);
    }

    // -------------------------------------------------------------------------
    // Méthodes utilitaires
    // -------------------------------------------------------------------------

    // Active ou désactive la visibilité du SpriteRenderer de la pièce.
    private void SetVisible(bool visible)
    {
        // Applique la visibilité si le SpriteRenderer est valide.
        if (_coinSpriteRenderer != null)
            _coinSpriteRenderer.enabled = visible;
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

    // Génère un sprite jaune d'un pixel pour le visuel de la pièce.
    private Sprite CreateFlatSprite()
    {
        // Crée une texture minimale d'un pixel de côté.
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, _coinColor);
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

