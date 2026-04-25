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

    // Initialise le sprite du joueur
    private void Awake()
    {
        BuildPlayerSprite();
    }

    // Applique uniquement le matériau et le sorting order — scale et sprite gérés ailleurs
    private void BuildPlayerSprite()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = gameObject.AddComponent<SpriteRenderer>();

        SetURPMaterial(sr);
        sr.sortingOrder = 3;
    }

    // Assigne le shader URP au SpriteRenderer donné
    private void SetURPMaterial(SpriteRenderer sr)
    {
        sr.sharedMaterial = new Material(
            Shader.Find(URPShaderName)
        );
    }
}
