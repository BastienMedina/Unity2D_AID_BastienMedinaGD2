using UnityEngine;

// Configure le rendu du virus rapide — le collider est défini directement dans le prefab
public class VirusFastVisual : MonoBehaviour
{
    // Ordre de tri dans le layer de rendu
    [SerializeField] private int _sortingOrder = 2;

    // Nom du shader URP 2D Sprite-Unlit-Default
    private const string ShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

    // Applique uniquement le shader et le sortingOrder — collider géré dans le prefab
    private void Awake()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>()
            ?? gameObject.AddComponent<SpriteRenderer>();

        sr.sharedMaterial = new Material(Shader.Find(ShaderName));
        sr.sortingOrder   = _sortingOrder;
    }
}
