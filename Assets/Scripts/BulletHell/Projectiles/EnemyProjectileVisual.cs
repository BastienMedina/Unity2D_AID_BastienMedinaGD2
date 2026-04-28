using UnityEngine;

public class EnemyProjectileVisual : MonoBehaviour
{
    [SerializeField] private int _sortingOrder = 3;
    [SerializeField] private float _spriteScale = 0.2f;

    private void Awake() // Applique sorting order et scale depuis l'inspecteur
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        sr.sortingOrder      = _sortingOrder;
        transform.localScale = new Vector3(_spriteScale, _spriteScale, 1f);
    }
}
