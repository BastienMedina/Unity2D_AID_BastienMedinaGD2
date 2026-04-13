using UnityEngine;

// Assigne les sprites visuels à chaque ennemi en Awake ou à la demande
[DefaultExecutionOrder(-5)]
public class EnemyVisualBuilder : MonoBehaviour
{
    // -----------------------------------------------------------------------
    // Constante shader URP partagée par tous les ennemis
    // -----------------------------------------------------------------------

    // Nom du shader URP 2D non-éclairé requis en URP
    private const string URPShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

    // -----------------------------------------------------------------------
    // API publique statique — appelée par ProceduralMapGenerator après Instantiate
    // -----------------------------------------------------------------------

    /// <summary>Applique les visuels de chargeur sur le GameObject fourni.</summary>
    public static void ApplyChargerVisual(GameObject go)
    {
        if (go == null) return;

        SpriteRenderer sr = GetOrAddSpriteRenderer(go);
        sr.sprite = CreateColorSprite(new Color(1f, 0.2f, 0.2f, 1f));
        SetURPMaterial(sr);
        sr.sortingOrder = 2;
        go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
    }

    /// <summary>Applique les visuels de tireur sur le GameObject fourni.</summary>
    public static void ApplyShooterVisual(GameObject go)
    {
        if (go == null) return;

        SpriteRenderer sr = GetOrAddSpriteRenderer(go);
        sr.sprite = CreateColorSprite(new Color(0.6f, 0.2f, 1f, 1f));
        SetURPMaterial(sr);
        sr.sortingOrder = 2;
        go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
    }

    /// <summary>Applique les visuels de l'ennemi caché sur le GameObject fourni.</summary>
    public static void ApplyHiddenVisual(GameObject go)
    {
        if (go == null) return;

        // --- Visuel caché : aspect bureau gris foncé ---
        Transform hiddenVisualTransform = go.transform.Find("HiddenVisual");
        if (hiddenVisualTransform != null)
        {
            SpriteRenderer srHidden = GetOrAddSpriteRenderer(hiddenVisualTransform.gameObject);
            srHidden.sprite = CreateColorSprite(new Color(0.27f, 0.27f, 0.27f, 1f));
            SetURPMaterial(srHidden);
            srHidden.sortingOrder = 1;
            hiddenVisualTransform.localScale = new Vector3(1.5f, 0.8f, 1f);
        }

        // --- Visuel ennemi révélé : sprite orange (désactivé au départ) ---
        Transform enemyVisualTransform = go.transform.Find("EnemyVisual");
        if (enemyVisualTransform != null)
        {
            SpriteRenderer srEnemy = GetOrAddSpriteRenderer(enemyVisualTransform.gameObject);
            srEnemy.sprite = CreateColorSprite(new Color(1f, 0.4f, 0f, 1f));
            SetURPMaterial(srEnemy);
            srEnemy.sortingOrder = 2;
            enemyVisualTransform.localScale = new Vector3(0.5f, 0.5f, 1f);
            enemyVisualTransform.gameObject.SetActive(false);
        }
    }

    /// <summary>Applique les visuels de spawner réseau sur le GameObject fourni.</summary>
    public static void ApplyNetworkSpawnerVisual(GameObject go)
    {
        if (go == null) return;

        SpriteRenderer sr = GetOrAddSpriteRenderer(go);
        sr.sprite = CreateColorSprite(new Color(0f, 1f, 1f, 1f));
        SetURPMaterial(sr);
        sr.sortingOrder = 2;
        go.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
    }

    // -----------------------------------------------------------------------
    // Cycle de vie Unity — câble les ennemis pré-placés dans la scène par nom
    // -----------------------------------------------------------------------

    // Point d'entrée : applique les visuels aux ennemis pré-placés
    private void Awake()
    {
        ApplyChargerVisual(GameObject.Find("Enemy_Charger_01"));
        ApplyShooterVisual(GameObject.Find("Enemy_Shooter_01"));
        ApplyHiddenVisual(GameObject.Find("Enemy_Hidden_01"));
        ApplyNetworkSpawnerVisual(GameObject.Find("Enemy_NetworkSpawner_01"));
    }

    // -----------------------------------------------------------------------
    // Helpers privés statiques
    // -----------------------------------------------------------------------

    // Récupère ou ajoute un SpriteRenderer sur le GameObject cible
    private static SpriteRenderer GetOrAddSpriteRenderer(GameObject go)
    {
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = go.AddComponent<SpriteRenderer>();
        return sr;
    }

    // Crée un sprite de couleur unie depuis une texture d'un pixel
    private static Sprite CreateColorSprite(Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.filterMode = FilterMode.Point;
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    // Assigne le shader URP au SpriteRenderer donné
    private static void SetURPMaterial(SpriteRenderer sr)
    {
        sr.sharedMaterial = new Material(Shader.Find(URPShaderName));
    }
}
