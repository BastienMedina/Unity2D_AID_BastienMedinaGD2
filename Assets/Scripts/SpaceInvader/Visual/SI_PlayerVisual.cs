using UnityEngine;

// Configure le rendu du vaisseau joueur sans écraser le sprite assigné dans le prefab
public class SI_PlayerVisual : MonoBehaviour
{
    // Ordre de tri dans le layer de rendu par défaut
    [SerializeField] private int _sortingOrder = 3;

    // Nom du shader URP 2D Sprite-Unlit-Default
    private const string ShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

    // Applique uniquement le shader et le sortingOrder — le sprite vient du prefab
    private void Awake()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>()
            ?? gameObject.AddComponent<SpriteRenderer>();

        sr.sharedMaterial = new Material(Shader.Find(ShaderName));
        sr.sortingOrder   = _sortingOrder;
    }
}
