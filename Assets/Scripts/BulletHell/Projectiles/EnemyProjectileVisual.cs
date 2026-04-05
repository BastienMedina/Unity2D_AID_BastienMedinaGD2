using UnityEngine;

// Initialise le visuel du projectile ennemi au démarrage
public class EnemyProjectileVisual : MonoBehaviour
{
    // Couleur rouge du projectile configurable dans l'inspecteur
    [SerializeField] private Color _projectileColor = new Color(1f, 0f, 0f, 1f);

    // Ordre de tri du sprite dans le rendu 2D
    [SerializeField] private int _sortingOrder = 3;

    // Échelle appliquée au projectile via le Transform local
    [SerializeField] private float _spriteScale = 0.2f;

    // Crée le sprite rouge au démarrage du prefab ennemi
    private void Awake()
    {
        // Récupère le SpriteRenderer existant sur le projectile
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // Ajoute un SpriteRenderer si absent du GameObject
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }

        // Génère la texture d'un pixel de couleur rouge
        Texture2D tex = new Texture2D(1, 1);
        tex.filterMode = FilterMode.Point;

        // Applique la couleur rouge à l'unique pixel de la texture
        tex.SetPixel(0, 0, _projectileColor);
        tex.Apply();

        // Crée et assigne le sprite depuis la texture générée
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        // Assigne le matériau URP non-éclairé au SpriteRenderer
        sr.sharedMaterial = new Material(
            Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
        );

        // Définit l'ordre de tri pour dessiner le projectile devant les ennemis
        sr.sortingOrder = _sortingOrder;

        // Applique la taille du projectile via le Transform local
        transform.localScale = new Vector3(_spriteScale, _spriteScale, 1f);
    }
}
