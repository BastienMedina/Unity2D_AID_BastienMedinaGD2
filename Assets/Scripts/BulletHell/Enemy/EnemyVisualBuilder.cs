using UnityEngine;

[DefaultExecutionOrder(-5)]
public class EnemyVisualBuilder : MonoBehaviour
{
    private const string URPShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

    public static void ApplyChargerVisual(GameObject go) // Applique le visuel du chargeur
    {
        if (go == null) return;
        SpriteRenderer sr = GetOrAddSpriteRenderer(go);
        SetURPMaterial(sr);
        sr.sortingOrder = 5;
    }

    public static void ApplyShooterVisual(GameObject go) // Applique le visuel du tireur
    {
        if (go == null) return;
        SpriteRenderer sr = GetOrAddSpriteRenderer(go);
        SetURPMaterial(sr);
        sr.sortingOrder = 5;
    }

    public static void ApplyHiddenVisual(GameObject go) // Applique les visuels de l'ennemi caché
    {
        if (go == null) return;

        Transform hiddenT = go.transform.Find("HiddenVisual");
        if (hiddenT != null) // Visuel bureau gris foncé
        {
            SpriteRenderer srH = GetOrAddSpriteRenderer(hiddenT.gameObject);
            srH.sprite = CreateColorSprite(new Color(0.27f, 0.27f, 0.27f, 1f));
            SetURPMaterial(srH);
            srH.sortingOrder = 1;
            hiddenT.localScale = new Vector3(1.5f, 0.8f, 1f);
        }

        Transform enemyT = go.transform.Find("EnemyVisual");
        if (enemyT != null) // Visuel ennemi orange (désactivé au départ)
        {
            SpriteRenderer srE = GetOrAddSpriteRenderer(enemyT.gameObject);
            srE.sprite = CreateColorSprite(new Color(1f, 0.4f, 0f, 1f));
            SetURPMaterial(srE);
            srE.sortingOrder = 2;
            enemyT.localScale = new Vector3(0.5f, 0.5f, 1f);
            enemyT.gameObject.SetActive(false);
        }
    }

    public static void ApplyNetworkSpawnerVisual(GameObject go) // Applique le visuel du spawner réseau
    {
        if (go == null) return;
        SpriteRenderer sr = GetOrAddSpriteRenderer(go);
        sr.sprite = CreateColorSprite(new Color(0f, 1f, 1f, 1f));
        SetURPMaterial(sr);
        sr.sortingOrder = 2;
        go.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
    }

    private void Awake() // Applique les visuels aux ennemis pré-placés dans la scène
    {
        ApplyChargerVisual(GameObject.Find("Enemy_Charger_01"));
        ApplyShooterVisual(GameObject.Find("Enemy_Shooter_01"));
        ApplyHiddenVisual(GameObject.Find("Enemy_Hidden_01"));
        ApplyNetworkSpawnerVisual(GameObject.Find("Enemy_NetworkSpawner_01"));
    }

    private static SpriteRenderer GetOrAddSpriteRenderer(GameObject go) // Récupère ou ajoute un SpriteRenderer
    {
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        return sr != null ? sr : go.AddComponent<SpriteRenderer>();
    }

    private static Sprite CreateColorSprite(Color color) // Crée un sprite de couleur unie 1px
    {
        Texture2D tex = new Texture2D(1, 1) { filterMode = FilterMode.Point };
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    private static void SetURPMaterial(SpriteRenderer sr) // Assigne le shader URP non-éclairé
    {
        sr.sharedMaterial = new Material(Shader.Find(URPShaderName));
    }
}
