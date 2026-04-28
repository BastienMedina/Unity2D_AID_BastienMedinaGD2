using UnityEngine;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private VirtualJoystick _virtualJoystick;
    [SerializeField] private float _deadZoneThreshold = 0.1f;
    [SerializeField] private UnityEvent<Vector2> _onPlayerMoved;

    private Rigidbody2D _rigidbody;
    private Vector2 _currentDirection = Vector2.zero;
    private Vector2 _lastFacingDirection = Vector2.up;

    private void Awake() // Initialise le Rigidbody2D et valide la référence
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        if (_rigidbody == null)
            Debug.LogError("[PlayerMovement] Rigidbody2D manquant — déplacement impossible.");
    }

    private void Start() // Cherche le joystick dans la scène si non assigné
    {
        if (_virtualJoystick == null) // Fallback par FindFirstObjectByType
        {
            _virtualJoystick = FindFirstObjectByType<VirtualJoystick>();
            if (_virtualJoystick == null)
                Debug.LogError("[PlayerMovement] VirtualJoystick introuvable dans la scène.");
        }
    }

    private void Update() // Lit la direction du joystick chaque frame
    {
        if (_virtualJoystick == null) return;

        Vector2 dir = _virtualJoystick.GetDirection();

        if (dir.magnitude < _deadZoneThreshold) // Ignore si sous le seuil minimal
        {
            _currentDirection = Vector2.zero;
            return;
        }

        _currentDirection = dir.normalized;
        _onPlayerMoved?.Invoke(transform.position);
    }

    private void FixedUpdate() // Applique la vélocité au Rigidbody en cadence physique
    {
        if (_rigidbody == null) return;

        _rigidbody.linearVelocity = _currentDirection * _moveSpeed;

        if (_currentDirection.magnitude >= _deadZoneThreshold) // Met à jour la direction de visée
            _lastFacingDirection = _currentDirection.normalized;
    }

    public Vector2 GetFacingDirection() => _lastFacingDirection; // Expose la dernière direction valide

    public bool IsMoving() => _currentDirection.magnitude >= _deadZoneThreshold; // Retourne vrai si en déplacement

    public void SetSpeed(float newSpeed) // Met à jour la vitesse de déplacement
    {
        _moveSpeed = newSpeed;
    }
}
