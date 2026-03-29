using UnityEngine;

// Construit le fond de la scène Space Invaders
public class SI_BackgroundBuilder : MonoBehaviour
{
    // Couleur du fond principal noir-bleuté
    [SerializeField] private Color _bgMainColor = new Color(0.04f, 0.04f, 0.1f, 1f);

    // Couleur de la ligne de sol grise
    [SerializeField] private Color _groundColor = new Color(0.2f, 0.2f, 0.3f, 1f);

    // Couleur de la ligne de plafond grise claire
    [SerializeField] private Color _ceilingColor = new Color(0.15f, 0.15f, 0.25f, 1f);

    // Nom du shader URP 2D utilisé pour tous les sprites
    private const string ShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

    // Initialise tous les éléments visuels du fond
    private void Awake()
    {
        // Crée le fond principal noir-bleu
        CreateSprite("BG_Main", _bgMainColor,
            new Vector3(0f, 0f, 2f), new Vector3(12f, 12f, 1f), -10);

        // Crée la ligne de sol en bas de l'écran
        CreateSprite("BG_Ground", _groundColor,
            new Vector3(0f, -4.2f, 0f), new Vector3(12f, 0.15f, 1f), -1);

        // Crée la ligne de plafond en haut de l'écran
        CreateSprite("BG_Ceiling", _ceilingColor,
            new Vector3(0f, 4.8f, 0f), new Vector3(12f, 0.15f, 1f), -1);

        // Crée le mur gauche invisible (collision)
        CreateWall("Wall_Left", new Vector3(-5.5f, 0f, 0f), new Vector3(0.2f, 12f, 1f));

        // Crée le mur droit invisible (collision)
        CreateWall("Wall_Right", new Vector3(5.5f, 0f, 0f), new Vector3(0.2f, 12f, 1f));
    }

    // Crée un sprite coloré avec les paramètres donnés
    private void CreateSprite(string goName, Color color,
        Vector3 pos, Vector3 scale, int sortingOrder)
    {
        // Instancie un GameObject vide nommé selon le paramètre
        GameObject go = new GameObject(goName);
        go.transform.position   = pos;
        go.transform.localScale = scale;

        // Ajoute le SpriteRenderer et configure la texture unie
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();

        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();

        // Crée le sprite à partir de la texture d'un pixel
        sr.sprite = Sprite.Create(tex,
            new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        // Assigne le shader URP 2D Sprite-Unlit-Default
        sr.sharedMaterial = new Material(Shader.Find(ShaderName));
        sr.sortingOrder   = sortingOrder;
    }

    // Crée un mur de collision invisible sur le layer Wall
    private void CreateWall(string goName, Vector3 pos, Vector3 scale)
    {
        // Instancie un GameObject vide pour le mur de collision
        GameObject go = new GameObject(goName);
        go.transform.position   = pos;
        go.transform.localScale = scale;

        // Résout l'index du layer Wall dans le projet
        int wallLayer = LayerMask.NameToLayer("Wall");

        // Affecte le layer seulement s'il existe dans le projet
        if (wallLayer >= 0)
        {
            go.layer = wallLayer;
        }
        else
        {
            // Signale que le layer Wall n'est pas créé dans le projet
            Debug.LogWarning("[SI] SI_BackgroundBuilder — layer 'Wall' introuvable, murs sans layer.");
        }

        go.AddComponent<BoxCollider2D>();
    }
}
