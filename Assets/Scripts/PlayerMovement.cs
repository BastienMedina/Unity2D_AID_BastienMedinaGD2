using UnityEngine;
using UnityEngine.Events;

// Gère uniquement le déplacement du joueur via joystick
public class PlayerMovement : MonoBehaviour
{
    // Vitesse de déplacement configurable dans l'inspecteur
    [SerializeField] private float _moveSpeed = 3f;

    // Référence au joystick virtuel mobile
    [SerializeField] private VirtualJoystick _virtualJoystick;

    // Seuil minimal pour ignorer les micro-inputs du joystick
    [SerializeField] private float _deadZoneThreshold = 0.1f;

    // Événement déclenché à chaque déplacement du joueur
    [SerializeField] private UnityEvent<Vector2> _onPlayerMoved;

    // Composant pour retourner le sprite horizontalement
    private SpriteRenderer _spriteRenderer;

    // Initialise les références de composants nécessaires
    private void Awake()
    {
        // Récupère le SpriteRenderer attaché au même objet
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Appelé à chaque frame pour traiter le mouvement
    private void Update()
    {
        // Lit la direction normalisée depuis le joystick virtuel
        Vector2 direction = _virtualJoystick.GetDirection();

        // Ignore les entrées trop faibles (zone morte du joystick)
        if (direction.magnitude < _deadZoneThreshold)
        {
            return;
        }

        // Normalise pour éviter un boost en diagonale
        direction = direction.normalized;

        // Calcule le déplacement selon la vitesse et le temps écoulé
        Vector2 movement = direction * (_moveSpeed * Time.deltaTime);

        // Déplace le joueur dans l'espace monde via Transform
        transform.Translate(movement, Space.World);

        // Retourne le sprite selon la direction horizontale
        FlipSprite(direction.x);

        // Notifie les abonnés de la nouvelle position du joueur
        _onPlayerMoved?.Invoke(transform.position);
    }

    // Retourne le sprite horizontalement selon le déplacement
    private void FlipSprite(float horizontalDirection)
    {
        // Oriente le sprite vers la gauche si direction négative
        if (horizontalDirection < 0f)
        {
            _spriteRenderer.flipX = true;
        }
        // Oriente le sprite vers la droite si direction positive
        else if (horizontalDirection > 0f)
        {
            _spriteRenderer.flipX = false;
        }
    }
}
