using UnityEngine;

// Configure le rendu du projectile ennemi sans écraser le sprite assigné dans le prefab.
public class EnemyProjectileVisual : MonoBehaviour
{
    // Ordre de tri du sprite dans le rendu 2D
    [SerializeField] private int _sortingOrder = 3;

    // Échelle appliquée au projectile via le Transform local
    [SerializeField] private float _spriteScale = 0.2f;

    // Applique uniquement le sortingOrder et le scale — le sprite vient du prefab
    private void Awake()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        sr.sortingOrder      = _sortingOrder;
        transform.localScale = new Vector3(_spriteScale, _spriteScale, 1f);
    }
}
