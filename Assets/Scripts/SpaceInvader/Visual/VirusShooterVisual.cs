using UnityEngine;

// Script autonome de visuel pour le virus tireur
public class VirusShooterVisual : MonoBehaviour
{
    // Couleur violette du virus tireur
    [SerializeField] private Color _color = new Color(0.8f, 0.4f, 1f, 1f);

    // Échelle horizontale du virus en unités monde
    [SerializeField] private float _scaleX = 0.35f;

    // Échelle verticale du virus en unités monde
    [SerializeField] private float _scaleY = 0.35f;

    // Ordre de tri dans le layer de rendu par défaut
    [SerializeField] private int _sortingOrder = 2;

    // Rayon local du collider pour obtenir 0.28u en espace monde
    [SerializeField] private float _colliderRadius = 0.8f;

    // Nom du shader URP 2D non éclairé utilisé partout
    private const string ShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

    // Initialise le visuel et le collider du virus tireur
    private void Awake()
    {
        // Construit le sprite coloré sur ce GameObject
        BuildSprite(_color, _scaleX, _scaleY, _sortingOrder);

        // Configure le collider trigger avec le rayon local calculé
        SetupCollider(_colliderRadius);
    }

    // Crée un sprite de couleur unie sur ce GameObject
    private void BuildSprite(Color color, float scaleX, float scaleY, int order)
    {
        // Récupère ou ajoute un SpriteRenderer
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

        // Génère une texture d'un pixel de la couleur voulue
        Texture2D tex = new Texture2D(1, 1);
        tex.filterMode = FilterMode.Point;
        tex.SetPixel(0, 0, color);
        tex.Apply();

        // Crée le sprite depuis la texture générée
        sr.sprite = Sprite.Create(
            tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        // Assigne le shader URP pour la visibilité
        sr.sharedMaterial = new Material(Shader.Find(ShaderName));

        // Définit l'ordre de rendu et l'échelle
        sr.sortingOrder = order;
        transform.localScale = new Vector3(scaleX, scaleY, 1f);
    }

    // Configure le CircleCollider2D trigger avec le rayon local fourni
    private void SetupCollider(float radius)
    {
        // Récupère ou ajoute un CircleCollider2D sur ce GameObject
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null) col = gameObject.AddComponent<CircleCollider2D>();

        // Active le mode trigger pour les détections sans physique rigide
        col.isTrigger = true;

        // Assigne le rayon exprimé en espace local du transform
        col.radius = radius;
    }
}
