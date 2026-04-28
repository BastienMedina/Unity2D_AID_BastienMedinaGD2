using UnityEngine;
using UnityEngine.Events;

public class SI_PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _minX = -4f;
    [SerializeField] private float _maxX = 4f;
    [SerializeField] private VirtualJoystick _virtualJoystick;
    [SerializeField] private UnityEvent<float> _onPositionChanged;

    private Rigidbody2D _rigidbody;
    private SpriteRenderer _spriteRenderer;
    private float _horizontalDirection;
    private float _fixedY;

    private void Awake() // Initialise les composants et la position de départ
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        if (_rigidbody == null)
            Debug.LogError("[PM] SI_PlayerMovement — Rigidbody2D manquant sur le joueur");

        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
            Debug.LogError("[PM] SI_PlayerMovement — SpriteRenderer manquant sur le joueur");

        if (_virtualJoystick == null)
        {
            _virtualJoystick = FindObjectOfType<VirtualJoystick>();
            if (_virtualJoystick == null)
                Debug.LogError("[PM] SI_PlayerMovement — Aucun VirtualJoystick trouvé dans la scène");
        }

        transform.position = new Vector2(0f, -3.0f);
        _fixedY = transform.position.y;
    }

    private void Update() // Lit la direction horizontale et retourne le sprite
    {
        if (_virtualJoystick == null) return;
        _horizontalDirection = _virtualJoystick.GetDirection().x;
        FlipSprite(_horizontalDirection);
    }

    private void FixedUpdate() // Applique le déplacement horizontal clamped
    {
        if (_rigidbody == null) return;

        float newX = _rigidbody.position.x + _horizontalDirection * _moveSpeed * Time.fixedDeltaTime;
        float clampedX = Mathf.Clamp(newX, _minX, _maxX); // Bloque dans la zone de jeu
        _rigidbody.MovePosition(new Vector2(clampedX, _fixedY));

        if (_horizontalDirection != 0f)
            _onPositionChanged?.Invoke(clampedX);
    }

    private void FlipSprite(float horizontalDirection) // Retourne le sprite selon la direction
    {
        if (_spriteRenderer == null) return;
        if      (horizontalDirection < 0f) _spriteRenderer.flipX = true;
        else if (horizontalDirection > 0f) _spriteRenderer.flipX = false;
    }
}
