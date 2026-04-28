using UnityEngine;

[DefaultExecutionOrder(-5)]
public class PrefabVisualBuilder : MonoBehaviour
{
    private const string URPShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

    private void Awake() // Applique les visuels à toutes les instances de prefabs
    {
        ApplyPrefabVisual("Barrel_Prefab",         new Color(1f,  0.55f, 0f,   1f), new Vector3(0.25f, 0.25f, 1f), sortingOrder: 3);
        ApplyPrefabVisual("EnemyProjectile_Prefab", new Color(1f,  0f,   0f,   1f), new Vector3(0.2f,  0.2f,  1f), sortingOrder: 3);
        ApplyPrefabVisual("NetworkUnit_Prefab",     new Color(0f,  0.8f, 0.8f, 1f), new Vector3(0.4f,  0.4f,  1f), sortingOrder: 2);
        ApplyPrefabVisual("Loot_Heal",             new Color(0f,  1f,   0.53f,1f), new Vector3(0.35f, 0.35f, 1f), sortingOrder: 2);
        ApplyPrefabVisual("Loot_PowerUp",          new Color(1f,  0.84f,0f,   1f), new Vector3(0.35f, 0.35f, 1f), sortingOrder: 2);
        ApplyPrefabVisual("Loot_HeroCartridge",    new Color(1f,  0.65f,0f,   1f), new Vector3(0.4f,  0.4f,  1f), sortingOrder: 2);
    }

    private void ApplyPrefabVisual(string goName, Color color, Vector3 scale, int sortingOrder) // Cherche, configure et redimensionne le prefab
    {
        GameObject go = GameObject.Find(goName);
        if (go == null) return;

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>() ?? go.AddComponent<SpriteRenderer>();
        sr.sprite       = CreateColorSprite(color);
        SetURPMaterial(sr);
        sr.sortingOrder = sortingOrder;
        go.transform.localScale = scale;
    }

    private Sprite CreateColorSprite(Color color) // Génère un sprite d'un pixel de la couleur donnée
    {
        Texture2D tex  = new Texture2D(1, 1);
        tex.filterMode = FilterMode.Point;
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    private void SetURPMaterial(SpriteRenderer sr) // Assigne le shader URP Sprite-Unlit-Default
    {
        sr.sharedMaterial = new Material(Shader.Find(URPShaderName));
    }
}
