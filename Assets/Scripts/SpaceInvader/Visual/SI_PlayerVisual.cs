using UnityEngine;

public class SI_PlayerVisual : MonoBehaviour
{
    [SerializeField] private int _sortingOrder = 3;

    private const string ShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

    private void Awake() // Applique shader et sortingOrder sans écraser le sprite du prefab
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();
        sr.sharedMaterial = new Material(Shader.Find(ShaderName));
        sr.sortingOrder   = _sortingOrder;
    }
}
