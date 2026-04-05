using UnityEngine;
using UnityEngine.InputSystem;

// Adapte l'entrée clavier et souris pour les tests
public class SI_EditorInputAdapter : MonoBehaviour
{
    // Référence au composant de déplacement du joueur
    [SerializeField] private SI_PlayerMovement _movement;

    // Référence au composant de tir du joueur
    [SerializeField] private SI_PlayerShooter _shooter;

    // Vitesse de déplacement horizontal en unités par seconde
    [SerializeField] private float _moveSpeed = 4f;

    // Lit les entrées clavier et souris à chaque frame
    private void Update()
    {
        // Lit l'axe horizontal clavier (A/D ou flèches)
        float h = 0f;

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
        {
            h = -1f;
        }

        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
        {
            h = 1f;
        }

        // Déplace directement le transform horizontalement
        transform.Translate(
            new Vector3(h * _moveSpeed * Time.deltaTime, 0f, 0f),
            Space.World);

        // Tire au clic gauche ou touche espace
        if (_shooter != null &&
            (Mouse.current.leftButton.wasPressedThisFrame ||
             Keyboard.current.spaceKey.wasPressedThisFrame))
        {
            // Confirme la détection du clic ou de l'espace
            Debug.Log("[SI] EditorInputAdapter — input tir détecté");

            // Vérifie la référence au shooter
            Debug.Log($"[SI] EditorInputAdapter — _shooter={_shooter != null}");

            // Déclenche un tir via le composant shooter du joueur
            _shooter.Shoot();
        }
    }
}
