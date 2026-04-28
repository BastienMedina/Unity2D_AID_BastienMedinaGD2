using UnityEngine;

public class VirusMiniVisual : MonoBehaviour
{
    [SerializeField] private int _sortingOrder = 2;

    private const string ShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

    private void Awake() // Applique shader et sortingOrder sans écraser le sprite du prefab
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();
        sr.sharedMaterial = new Material(Shader.Find(ShaderName));
        sr.sortingOrder   = _sortingOrder;
    }
}
