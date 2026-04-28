using UnityEngine;

public class BarrelVisual : MonoBehaviour
{
    [SerializeField] private Color _barrelColor = new Color(1f, 0.55f, 0f, 1f);
    [SerializeField] private int _sortingOrder = 3;
    [SerializeField] private float _spriteScale = 0.25f;

    private void Awake() // Crée le sprite orange au démarrage
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = gameObject.AddComponent<SpriteRenderer>();

        Texture2D tex = new Texture2D(1, 1);
        tex.filterMode = FilterMode.Point;
        tex.SetPixel(0, 0, _barrelColor);
        tex.Apply();

        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        sr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
        sr.sortingOrder = _sortingOrder;

        transform.localScale = new Vector3(_spriteScale, _spriteScale, 1f);
    }
}
