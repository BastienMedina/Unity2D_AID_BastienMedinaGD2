using UnityEngine;

// Initialise le visuel de l'unité réseau ennemie au démarrage
public class NetworkUnitVisual : MonoBehaviour
{
    // Couleur cyan de l'unité réseau configurable dans l'inspecteur
    [SerializeField] private Color _unitColor = new Color(0f, 0.8f, 0.8f, 1f);

    // Ordre de tri du sprite dans le rendu 2D
    [SerializeField] private int _sortingOrder = 2;

    // Échelle appliquée à l'unité via le Transform local
    [SerializeField] private float _spriteScale = 0.4f;

    // Crée le sprite cyan au démarrage du prefab unité réseau
    private void Awake()
    {
        // Récupère le SpriteRenderer existant sur l'unité réseau
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // Ajoute un SpriteRenderer si absent du GameObject
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }

        // Génère la texture d'un pixel de couleur cyan
        Texture2D tex = new Texture2D(1, 1);
        tex.filterMode = FilterMode.Point;

        // Applique la couleur cyan à l'unique pixel de la texture
        tex.SetPixel(0, 0, _unitColor);
        tex.Apply();

        // Crée et assigne le sprite depuis la texture générée
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        // Assigne le matériau URP non-éclairé au SpriteRenderer
        sr.sharedMaterial = new Material(
            Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
        );

        // Définit l'ordre de tri pour dessiner l'unité au bon niveau
        sr.sortingOrder = _sortingOrder;

        // Applique la taille de l'unité réseau via le Transform local
        transform.localScale = new Vector3(_spriteScale, _spriteScale, 1f);
    }
}
