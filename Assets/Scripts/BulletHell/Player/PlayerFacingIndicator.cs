using UnityEngine;

public class PlayerFacingIndicator : MonoBehaviour
{
    [SerializeField] private PlayerMovement _playerMovement;

    private void Update() // Oriente l'indicateur selon la direction du joueur
    {
        if (_playerMovement == null)
            return;

        Vector2 facing = _playerMovement.GetFacingDirection();

        if (facing.magnitude < 0.1f) // Ignore si direction trop faible
            return;

        float angle        = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
}
