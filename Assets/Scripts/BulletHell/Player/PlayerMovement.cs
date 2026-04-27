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

    // Rigidbody2D utilisé pour déplacer le joueur via la physique
    private Rigidbody2D _rigidbody;

    // Direction issue du joystick, partagée entre Update et FixedUpdate
    private Vector2 _currentDirection = Vector2.zero;

    // Stocke la dernière direction de déplacement valide
    private Vector2 _lastFacingDirection = Vector2.up;

    // Initialise les références de composants nécessaires
    private void Awake()
    {
        // Récupère le Rigidbody2D pour le déplacement physique
        _rigidbody = GetComponent<Rigidbody2D>();

        // Signale si le Rigidbody2D est absent du GameObject joueur
        if (_rigidbody == null)
        {
            Debug.LogError("[PM] Rigidbody2D manquant sur le joueur — déplacement impossible");
        }
    }

    // Cherche le joystick dans la scène si non assigné en Inspector
    private void Start()
    {
        // Tente de résoudre automatiquement la référence joystick manquante
        if (_virtualJoystick == null)
        {
            _virtualJoystick = FindFirstObjectByType<VirtualJoystick>();

            if (_virtualJoystick == null)
                Debug.LogError("[PlayerMovement] VirtualJoystick introuvable dans la scène — le joueur ne pourra pas se déplacer.");
        }
    }

    // Lit la direction du joystick et met à jour le sprite chaque frame
    private void Update()
    {
        // Abandonne si la référence joystick est toujours absente
        if (_virtualJoystick == null) return;

        // Lit la direction normalisée depuis le joystick virtuel
        Vector2 dir = _virtualJoystick.GetDirection();

        // Remet la direction à zéro si sous le seuil minimal
        if (dir.magnitude < _deadZoneThreshold)
        {
            _currentDirection = Vector2.zero;
            return;
        }

        // Normalise pour éviter le boost en diagonale
        _currentDirection = dir.normalized;

        // Notifie les abonnés de la nouvelle position du joueur
        _onPlayerMoved?.Invoke(transform.position);
    }

    // Applique le déplacement via Rigidbody2D en cadence physique
    private void FixedUpdate()
    {
        // Ignore si le Rigidbody2D est absent du joueur
        if (_rigidbody == null) return;

        // Applique la vélocité au Rigidbody2D selon la direction courante
        _rigidbody.linearVelocity = _currentDirection * _moveSpeed;

        // Met à jour la direction de visée si le joueur bouge
        if (_currentDirection.magnitude >= _deadZoneThreshold)
            _lastFacingDirection = _currentDirection.normalized;
    }

    // Retourne la dernière direction de déplacement valide
    public Vector2 GetFacingDirection() => _lastFacingDirection;

    /// <summary>Retourne vrai si le joueur se déplace activement cette frame.</summary>
    public bool IsMoving() => _currentDirection.magnitude >= _deadZoneThreshold;

    // Modifie la vitesse de déplacement depuis l'extérieur
    public void SetSpeed(float newSpeed)
    {
        // Met à jour la vitesse avec la nouvelle valeur
        _moveSpeed = newSpeed;
    }
}
