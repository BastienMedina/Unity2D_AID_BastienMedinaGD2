using UnityEngine;

public class SI_BackgroundBuilder : MonoBehaviour
{
    [SerializeField] private Color _bgMainColor  = new Color(0.04f, 0.04f, 0.1f, 1f);
    [SerializeField] private Color _groundColor  = new Color(0.2f, 0.2f, 0.3f, 1f);
    [SerializeField] private Color _ceilingColor = new Color(0.15f, 0.15f, 0.25f, 1f);

    private const string ShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

    private void Awake() // Construit le fond, le sol, le plafond et les murs latéraux
    {
        CreateSprite("BG_Main",    _bgMainColor,  new Vector3(0f,  0f,   2f), new Vector3(12f, 12f,   1f), -10);
        CreateSprite("BG_Ground",  _groundColor,  new Vector3(0f, -4.2f, 0f), new Vector3(12f, 0.15f, 1f), -1);
        CreateSprite("BG_Ceiling", _ceilingColor, new Vector3(0f,  4.8f, 0f), new Vector3(12f, 0.15f, 1f), -1);
        CreateWall("Wall_Left",  new Vector3(-5.5f, 0f, 0f), new Vector3(0.2f, 12f, 1f)); // Mur gauche
        CreateWall("Wall_Right", new Vector3( 5.5f, 0f, 0f), new Vector3(0.2f, 12f, 1f)); // Mur droit
    }

    private void CreateSprite(string goName, Color color, Vector3 pos, Vector3 scale, int sortingOrder) // Instancie un sprite coloré à une position donnée
    {
        GameObject go = new GameObject(goName);
        go.transform.position   = pos;
        go.transform.localScale = scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        Texture2D tex     = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        sr.sprite         = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        sr.sharedMaterial = new Material(Shader.Find(ShaderName));
        sr.sortingOrder   = sortingOrder;
    }

    private void CreateWall(string goName, Vector3 pos, Vector3 scale) // Instancie un mur de collision sur le layer Wall
    {
        GameObject go = new GameObject(goName);
        go.transform.position   = pos;
        go.transform.localScale = scale;

        int wallLayer = LayerMask.NameToLayer("Wall");
        if (wallLayer >= 0) go.layer = wallLayer;
        else Debug.LogWarning("[SI] SI_BackgroundBuilder — layer 'Wall' introuvable.");

        go.AddComponent<BoxCollider2D>();
    }
}
