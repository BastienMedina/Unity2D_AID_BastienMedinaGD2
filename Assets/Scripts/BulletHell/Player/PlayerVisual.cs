using UnityEngine;

[DefaultExecutionOrder(-20)]
public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private Color _playerColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private int _sortingOrder = 3;
    [SerializeField] private float _spriteScale = 0.6f;
}
