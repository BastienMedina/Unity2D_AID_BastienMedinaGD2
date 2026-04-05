using UnityEngine;

// Crée le sprite bleu du serveur à défendre
public class SI_ServerVisual : MonoBehaviour
{
    // Couleur bleue représentant le serveur à défendre
    [SerializeField] private Color _serverColor = new Color(0.2f, 0.5f, 1f, 1f);

    // Ordre de tri dans le layer de rendu par défaut
    [SerializeField] private int _sortingOrder = 2;

    // Nom du shader URP 2D Sprite-Unlit-Default
    private const string ShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

    // Initialise le sprite bleu sur le SpriteRenderer du serveur
    private void Awake()
    {
        // Récupère ou ajoute le SpriteRenderer sur ce GameObject
        SpriteRenderer sr = GetComponent<SpriteRenderer>()
            ?? gameObject.AddComponent<SpriteRenderer>();

        // Crée une texture 1x1 de la couleur du serveur
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, _serverColor);
        tex.Apply();

        // Crée le sprite à partir de la texture d'un pixel
        sr.sprite = Sprite.Create(tex,
            new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        // Assigne le shader URP 2D Sprite-Unlit-Default
        sr.sharedMaterial = new Material(Shader.Find(ShaderName));
        sr.sortingOrder   = _sortingOrder;
    }
}
