using UnityEngine;

// Fait pivoter l'indicateur de direction selon le déplacement du joueur
public class PlayerFacingIndicator : MonoBehaviour
{
    // Référence au script de déplacement pour la direction
    [SerializeField] private PlayerMovement _playerMovement;

    // Oriente l'indicateur selon la direction du déplacement chaque frame
    private void Update()
    {
        // Ignore si le script de déplacement n'est pas assigné
        if (_playerMovement == null)
        {
            return;
        }

        // Récupère la direction actuelle du joueur
        Vector2 facing = _playerMovement.GetFacingDirection();

        // Ignore si la direction est nulle ou trop faible
        if (facing.magnitude < 0.1f)
        {
            return;
        }

        // Calcule l'angle de rotation depuis la direction
        float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;

        // Applique la rotation à l'indicateur de direction
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
}
