using UnityEngine;
using UnityEngine.Events;

// Gère uniquement le déplacement horizontal du joueur
public class SI_PlayerMovement : MonoBehaviour
{
    // Vitesse de déplacement horizontal configurable
    [SerializeField] private float _moveSpeed = 4f;

    // Limite gauche de la zone de déplacement
    [SerializeField] private float _minX = -4f;

    // Limite droite de la zone de déplacement
    [SerializeField] private float _maxX = 4f;

    // Référence au joystick virtuel mobile
    [SerializeField] private VirtualJoystick _virtualJoystick;

    // Événement déclenché à chaque déplacement horizontal
    [SerializeField] private UnityEvent<float> _onPositionChanged;

    // Rigidbody2D utilisé pour déplacer le joueur physiquement
    private Rigidbody2D _rigidbody;

    // SpriteRenderer pour retourner le sprite horizontalement
    private SpriteRenderer _spriteRenderer;

    // Direction horizontale lue depuis le joystick chaque frame
    private float _horizontalDirection;

    // Position Y fixe du joueur, jamais modifiée
    private float _fixedY;

    // Initialise les composants et la position de départ
    private void Awake()
    {
        // Récupère le Rigidbody2D requis pour MovePosition
        _rigidbody = GetComponent<Rigidbody2D>();

        // Signale si le Rigidbody2D est manquant sur le joueur
        if (_rigidbody == null)
        {
            Debug.LogError("[PM] SI_PlayerMovement — Rigidbody2D manquant sur le joueur");
        }

        // Récupère le SpriteRenderer pour le retournement horizontal
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // Signale si le SpriteRenderer est manquant sur le joueur
        if (_spriteRenderer == null)
        {
            Debug.LogError("[PM] SI_PlayerMovement — SpriteRenderer manquant sur le joueur");
        }

        // Place le joueur au centre bas de l'écran au démarrage
        transform.position = new Vector2(0f, -3.5f);

        // Mémorise la position Y fixe pour ne jamais la modifier
        _fixedY = transform.position.y;
    }

    // Lit la direction horizontale du joystick chaque frame
    private void Update()
    {
        // Vérifie que la référence joystick est assignée dans l'inspecteur
        if (_virtualJoystick == null)
        {
            Debug.LogError("[PM] SI_PlayerMovement — _virtualJoystick non assigné dans l'inspecteur");
            return;
        }

        // Extrait uniquement la composante horizontale du joystick
        _horizontalDirection = _virtualJoystick.GetDirection().x;

        // Retourne le sprite selon la direction horizontale courante
        FlipSprite(_horizontalDirection);
    }

    // Applique le déplacement horizontal en cadence physique
    private void FixedUpdate()
    {
        // Abandonne si le Rigidbody2D est absent du joueur
        if (_rigidbody == null)
        {
            return;
        }

        // Calcule la nouvelle position X selon la vitesse et le temps
        float newX = _rigidbody.position.x + _horizontalDirection * _moveSpeed * Time.fixedDeltaTime;

        // Bloque la position X entre les limites gauche et droite
        float clampedX = Mathf.Clamp(newX, _minX, _maxX);

        // Construit la position cible en conservant le Y fixe
        Vector2 targetPosition = new Vector2(clampedX, _fixedY);

        // Déplace le joueur vers la position cible via la physique
        _rigidbody.MovePosition(targetPosition);

        // Déclenche l'événement si le joueur a bougé horizontalement
        if (_horizontalDirection != 0f)
        {
            // Notifie les abonnés avec la position X actuelle
            _onPositionChanged?.Invoke(clampedX);
        }
    }

    // Retourne le sprite selon la direction horizontale reçue
    private void FlipSprite(float horizontalDirection)
    {
        // Abandonne si le SpriteRenderer est absent du joueur
        if (_spriteRenderer == null)
        {
            return;
        }

        // Retourne le sprite vers la gauche si direction négative
        if (horizontalDirection < 0f)
        {
            _spriteRenderer.flipX = true;
        }
        // Retourne le sprite vers la droite si direction positive
        else if (horizontalDirection > 0f)
        {
            _spriteRenderer.flipX = false;
        }
    }
}
