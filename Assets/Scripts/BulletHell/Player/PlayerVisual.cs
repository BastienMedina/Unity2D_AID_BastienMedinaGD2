using UnityEngine;

// Initialise le visuel sprite du joueur directement sur le GameObject
[DefaultExecutionOrder(-20)]
public class PlayerVisual : MonoBehaviour
{
    // Couleur blanche du joueur configurable dans l'inspecteur
    [SerializeField] private Color _playerColor = new Color(1f, 1f, 1f, 1f);

    // Ordre de tri du sprite du joueur dans le rendu 2D
    [SerializeField] private int _sortingOrder = 3;

    // Échelle appliquée au joueur via son Transform local
    [SerializeField] private float _spriteScale = 0.6f;

    // Crée le sprite blanc du joueur au démarrage
    private void Awake()
    {
        // Récupère le SpriteRenderer existant sur le joueur
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // Ajoute un SpriteRenderer si le joueur n'en possède pas
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }

        // Génère la texture d'un pixel de couleur blanche
        Texture2D tex = new Texture2D(1, 1);
        tex.filterMode = FilterMode.Point;

        // Applique la couleur blanche à l'unique pixel de la texture
        tex.SetPixel(0, 0, _playerColor);
        tex.Apply();

        // Crée et assigne le sprite joueur depuis la texture générée
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        // Assigne le matériau URP non-éclairé pour garantir la visibilité
        sr.sharedMaterial = new Material(
            Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
        );

        // Définit l'ordre de tri pour dessiner le joueur devant le décor
        sr.sortingOrder = _sortingOrder;

        // Applique l'échelle du joueur via son Transform local
        transform.localScale = new Vector3(_spriteScale, _spriteScale, 1f);
    }
}
