using UnityEngine;

// Assigne les sprites visuels à chaque ennemi en Awake
[DefaultExecutionOrder(-5)]
public class EnemyVisualBuilder : MonoBehaviour
{
    // -----------------------------------------------------------------------
    // Constante shader URP partagée par tous les ennemis
    // -----------------------------------------------------------------------

    // Nom du shader URP 2D non-éclairé requis en URP
    private const string URPShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

    // -----------------------------------------------------------------------
    // Cycle de vie Unity
    // -----------------------------------------------------------------------

    // Point d'entrée : applique les visuels à tous les ennemis de la scène
    private void Awake()
    {
        BuildCharger();
        BuildShooter();
        BuildHidden();
        BuildNetworkSpawner();
    }

    // -----------------------------------------------------------------------
    // Construction visuelle par type d'ennemi
    // -----------------------------------------------------------------------

    // Applique un sprite rouge au Enemy_Charger_01
    private void BuildCharger()
    {
        // Cherche l'ennemi chargeur dans la scène par son nom
        GameObject go = GameObject.Find("Enemy_Charger_01");
        if (go == null)
        {
            Debug.LogWarning("[EnemyVisualBuilder] Enemy_Charger_01 introuvable dans la scène.");
            return;
        }

        // Couleur rouge vif pour le chargeur
        SpriteRenderer sr = GetOrAddSpriteRenderer(go);
        sr.sprite = CreateColorSprite(new Color(1f, 0.2f, 0.2f, 1f));
        SetURPMaterial(sr);
        sr.sortingOrder = 2;

        // Redimensionne l'ennemi chargeur via son Transform local
        go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
    }

    // Applique un sprite violet au Enemy_Shooter_01
    private void BuildShooter()
    {
        // Cherche le tireur dans la scène par son nom
        GameObject go = GameObject.Find("Enemy_Shooter_01");
        if (go == null)
        {
            Debug.LogWarning("[EnemyVisualBuilder] Enemy_Shooter_01 introuvable dans la scène.");
            return;
        }

        // Couleur violette pour le tireur
        SpriteRenderer sr = GetOrAddSpriteRenderer(go);
        sr.sprite = CreateColorSprite(new Color(0.6f, 0.2f, 1f, 1f));
        SetURPMaterial(sr);
        sr.sortingOrder = 2;

        // Redimensionne l'ennemi tireur via son Transform local
        go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
    }

    // Applique les visuels de bureau caché et d'ennemi orange au Hidden
    private void BuildHidden()
    {
        // Cherche l'ennemi caché dans la scène par son nom
        GameObject go = GameObject.Find("Enemy_Hidden_01");
        if (go == null)
        {
            Debug.LogWarning("[EnemyVisualBuilder] Enemy_Hidden_01 introuvable dans la scène.");
            return;
        }

        // --- Visuel caché : aspect bureau gris foncé ---
        Transform hiddenVisualTransform = go.transform.Find("HiddenVisual");
        if (hiddenVisualTransform != null)
        {
            // Couleur gris foncé pour simuler un bureau camouflage
            SpriteRenderer srHidden = GetOrAddSpriteRenderer(hiddenVisualTransform.gameObject);
            srHidden.sprite = CreateColorSprite(new Color(0.27f, 0.27f, 0.27f, 1f));
            SetURPMaterial(srHidden);
            srHidden.sortingOrder = 1;

            // Taille bureau pour le visuel caché
            hiddenVisualTransform.localScale = new Vector3(1.5f, 0.8f, 1f);
        }
        else
        {
            Debug.LogWarning("[EnemyVisualBuilder] HiddenVisual introuvable sous Enemy_Hidden_01.");
        }

        // --- Visuel ennemi révélé : sprite orange ---
        Transform enemyVisualTransform = go.transform.Find("EnemyVisual");
        if (enemyVisualTransform != null)
        {
            // Couleur orange pour l'ennemi une fois révélé
            SpriteRenderer srEnemy = GetOrAddSpriteRenderer(enemyVisualTransform.gameObject);
            srEnemy.sprite = CreateColorSprite(new Color(1f, 0.4f, 0f, 1f));
            SetURPMaterial(srEnemy);
            srEnemy.sortingOrder = 2;

            // Taille réduite pour le sprite de l'ennemi révélé
            enemyVisualTransform.localScale = new Vector3(0.5f, 0.5f, 1f);

            // Désactive le visuel ennemi au démarrage (caché par défaut)
            enemyVisualTransform.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[EnemyVisualBuilder] EnemyVisual introuvable sous Enemy_Hidden_01.");
        }
    }

    // Applique un sprite cyan au spawner réseau et à ses écrans
    private void BuildNetworkSpawner()
    {
        // Cherche le spawner réseau dans la scène par son nom
        GameObject go = GameObject.Find("Enemy_NetworkSpawner_01");
        if (go == null)
        {
            Debug.LogWarning("[EnemyVisualBuilder] Enemy_NetworkSpawner_01 introuvable dans la scène.");
            return;
        }

        // Couleur cyan pour le spawner réseau
        SpriteRenderer sr = GetOrAddSpriteRenderer(go);
        sr.sprite = CreateColorSprite(new Color(0f, 1f, 1f, 1f));
        SetURPMaterial(sr);
        sr.sortingOrder = 2;

        // Redimensionne le spawner réseau via son Transform local
        go.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
    }

    // -----------------------------------------------------------------------
    // Helpers sprite, matériau et composants
    // -----------------------------------------------------------------------

    // Récupère ou ajoute un SpriteRenderer sur le GameObject cible
    private SpriteRenderer GetOrAddSpriteRenderer(GameObject go)
    {
        // Retourne l'existant ou en crée un nouveau si absent
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = go.AddComponent<SpriteRenderer>();
        return sr;
    }

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
