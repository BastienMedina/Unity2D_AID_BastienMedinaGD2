using UnityEngine;

// Assigne les sprites à toutes les instances de prefabs en Awake
[DefaultExecutionOrder(-5)]
public class PrefabVisualBuilder : MonoBehaviour
{
    // -----------------------------------------------------------------------
    // Constante shader URP partagée par tous les prefabs
    // -----------------------------------------------------------------------

    // Nom du shader URP 2D non-éclairé requis en URP
    private const string URPShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

    // -----------------------------------------------------------------------
    // Cycle de vie Unity
    // -----------------------------------------------------------------------

    // Applique les visuels à toutes les instances de prefabs au démarrage
    private void Awake()
    {
        ApplyPrefabVisual("Barrel_Prefab",
            new Color(1f, 0.55f, 0f, 1f),
            new Vector3(0.25f, 0.25f, 1f),
            sortingOrder: 3);

        ApplyPrefabVisual("EnemyProjectile_Prefab",
            new Color(1f, 0f, 0f, 1f),
            new Vector3(0.2f, 0.2f, 1f),
            sortingOrder: 3);

        ApplyPrefabVisual("NetworkUnit_Prefab",
            new Color(0f, 0.8f, 0.8f, 1f),
            new Vector3(0.4f, 0.4f, 1f),
            sortingOrder: 2);

        ApplyPrefabVisual("Loot_Heal",
            new Color(0f, 1f, 0.53f, 1f),
            new Vector3(0.35f, 0.35f, 1f),
            sortingOrder: 2);

        ApplyPrefabVisual("Loot_PowerUp",
            new Color(1f, 0.84f, 0f, 1f),
            new Vector3(0.35f, 0.35f, 1f),
            sortingOrder: 2);

        ApplyPrefabVisual("Loot_HeroCartridge",
            new Color(1f, 0.65f, 0f, 1f),
            new Vector3(0.4f, 0.4f, 1f),
            sortingOrder: 2);
    }

    // -----------------------------------------------------------------------
    // Application du visuel sur une instance de prefab
    // -----------------------------------------------------------------------

    // Cherche le prefab par nom et lui assigne le sprite et la taille
    private void ApplyPrefabVisual(string goName, Color color,
        Vector3 scale, int sortingOrder)
    {
        // Cherche l'instance du prefab dans la scène par son nom exact
        GameObject go = GameObject.Find(goName);
        if (go == null)
        {
            Debug.LogWarning($"[PrefabVisualBuilder] {goName} introuvable dans la scène.");
            return;
        }

        // Récupère ou ajoute un SpriteRenderer sur le prefab
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = go.AddComponent<SpriteRenderer>();

        // Assigne le sprite de couleur unie au prefab
        sr.sprite = CreateColorSprite(color);
        SetURPMaterial(sr);
        sr.sortingOrder = sortingOrder;

        // Applique la taille du prefab via son Transform local
        go.transform.localScale = scale;
    }

    // -----------------------------------------------------------------------
    // Helpers sprite et matériau
    // -----------------------------------------------------------------------

    // Crée un sprite de couleur unie depuis une texture d'un pixel
    private Sprite CreateColorSprite(Color color)
    {
        // Génère une texture d'un pixel de côté
        Texture2D tex = new Texture2D(1, 1);
        tex.filterMode = FilterMode.Point;
        tex.SetPixel(0, 0, color);
        tex.Apply();

        // Retourne le sprite depuis la texture générée
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    // Assigne le shader URP au SpriteRenderer donné
    private void SetURPMaterial(SpriteRenderer sr)
    {
        sr.sharedMaterial = new Material(
            Shader.Find(URPShaderName)
        );
    }
}
