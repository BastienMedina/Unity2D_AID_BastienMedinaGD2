using UnityEngine;

// Script autonome de visuel pour le virus rapide
public class VirusFastVisual : MonoBehaviour
{
    // Couleur rouge vif du virus rapide
    [SerializeField] private Color _color = new Color(1f, 0.3f, 0.3f, 1f);

    // Échelle horizontale du virus en unités monde
    [SerializeField] private float _scaleX = 0.3f;

    // Échelle verticale du virus en unités monde
    [SerializeField] private float _scaleY = 0.3f;

    // Ordre de tri dans le layer de rendu par défaut
    [SerializeField] private int _sortingOrder = 2;

    // Nom du shader URP 2D non éclairé utilisé partout
    private const string ShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

    // Initialise le visuel du virus rapide rouge
    private void Awake() =>
        BuildSprite(_color, _scaleX, _scaleY, _sortingOrder);

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
}
