using UnityEngine;

[DefaultExecutionOrder(-5)]
public class PlayerVisualBuilder : MonoBehaviour
{
    private const string URPShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

    private void Awake() // Initialise le sprite et le matériau du joueur
    {
        BuildPlayerSprite();
    }

    private void BuildPlayerSprite() // Applique matériau URP et sorting order
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = gameObject.AddComponent<SpriteRenderer>();

        SetURPMaterial(sr);
        sr.sortingOrder = 3;
    }

    private void SetURPMaterial(SpriteRenderer sr) // Assigne le shader URP non-éclairé
    {
        sr.sharedMaterial = new Material(Shader.Find(URPShaderName));
    }
}
