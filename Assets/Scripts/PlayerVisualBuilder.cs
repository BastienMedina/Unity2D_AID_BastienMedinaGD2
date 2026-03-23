using UnityEngine;

// Crée et maintient le visuel sprite du joueur
[DefaultExecutionOrder(-5)]
public class PlayerVisualBuilder : MonoBehaviour
{
    // -----------------------------------------------------------------------
    // Constante shader URP pour les matériaux du joueur
    // -----------------------------------------------------------------------

    // Nom du shader URP 2D non-éclairé requis en URP
    private const string URPShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

    // -----------------------------------------------------------------------
    // Cycle de vie Unity
    // -----------------------------------------------------------------------

    // Initialise le sprite du joueur et de l'indicateur de direction
    private void Awake()
    {
        BuildPlayerSprite();
        BuildFacingIndicator();
    }

    // -----------------------------------------------------------------------
    // Construction du sprite principal du joueur
    // -----------------------------------------------------------------------

    // Applique un sprite blanc au SpriteRenderer du joueur
    private void BuildPlayerSprite()
    {
        // Récupère ou ajoute un SpriteRenderer sur le joueur
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = gameObject.AddComponent<SpriteRenderer>();

        // Assigne un sprite blanc uni au joueur
        sr.sprite = CreateColorSprite(Color.white);
        SetURPMaterial(sr);
        sr.sortingOrder = 3;

        // Redimensionne le joueur via son Transform local
        transform.localScale = new Vector3(0.6f, 0.6f, 1f);
    }

    // -----------------------------------------------------------------------
    // Construction de l'indicateur de direction
    // -----------------------------------------------------------------------

    // Crée ou configure l'indicateur de direction enfant du joueur
    private void BuildFacingIndicator()
    {
        // Cherche l'enfant FacingIndicator existant dans la hiérarchie
        Transform indicatorTransform = null;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).name == "FacingIndicator")
            {
                indicatorTransform = transform.GetChild(i);
                break;
            }
        }

        // Crée le FacingIndicator s'il n'existe pas encore dans la scène
        if (indicatorTransform == null)
        {
            GameObject indicatorGO = new GameObject("FacingIndicator");
            indicatorGO.transform.SetParent(transform, false);
            indicatorTransform = indicatorGO.transform;
        }

        // Positionne l'indicateur à droite du centre du joueur
        indicatorTransform.localPosition = new Vector3(0.4f, 0f, 0f);

        // Redimensionne l'indicateur en unités locales
        indicatorTransform.localScale = new Vector3(0.2f, 0.2f, 1f);

        // Récupère ou ajoute un SpriteRenderer sur l'indicateur
        SpriteRenderer sr = indicatorTransform.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = indicatorTransform.gameObject.AddComponent<SpriteRenderer>();

        // Couleur jaune dorée pour l'indicateur de direction
        sr.sprite = CreateColorSprite(new Color(1f, 0.84f, 0f, 1f));
        SetURPMaterial(sr);
        sr.sortingOrder = 4;
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
