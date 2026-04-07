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

    // Vitesse de rotation en degrés par seconde
    [SerializeField] private float _rotationSpeed = 720f;

    // Active l'interpolation de rotation si vrai, sinon instantané
    [SerializeField] private bool _smoothRotation = false;

    // Événement déclenché à chaque déplacement du joueur
    [SerializeField] private UnityEvent<Vector2> _onPlayerMoved;

    // Composant pour retourner le sprite horizontalement
    private SpriteRenderer _spriteRenderer;

    // Rigidbody2D utilisé pour déplacer le joueur via la physique
    private Rigidbody2D _rigidbody;

    // Direction issue du joystick, partagée entre Update et FixedUpdate
    private Vector2 _currentDirection = Vector2.zero;

    // Stocke la dernière direction de déplacement valide
    private Vector2 _lastFacingDirection = Vector2.up;

    // Initialise les références de composants nécessaires
    private void Awake()
    {
        // Récupère le SpriteRenderer attaché au même objet
        _spriteRenderer = GetComponent<SpriteRenderer>();

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
            _virtualJoystick = FindObjectOfType<VirtualJoystick>();

            if (_virtualJoystick == null)
                Debug.LogError("[PlayerMovement] VirtualJoystick introuvable dans la scène.");
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

        // Retourne le sprite selon la direction horizontale courante
        FlipSprite(_currentDirection.x);

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

        // Ignore la rotation si le joueur est immobile
        if (_currentDirection.magnitude < _deadZoneThreshold) return;

        // Calcule l'angle cible depuis la direction de déplacement
        float targetAngle = Mathf.Atan2(_currentDirection.y, _currentDirection.x)
                            * Mathf.Rad2Deg - 90f;

        // Choisit entre rotation lisse ou instantanée selon le réglage
        if (_smoothRotation)
        {
            float currentAngle = _rigidbody.rotation;
            float nextAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle,
                                  _rotationSpeed * Time.fixedDeltaTime);
            _rigidbody.MoveRotation(nextAngle);
        }
        else
        {
            _rigidbody.MoveRotation(targetAngle);
        }
    }

    // Retourne le sprite horizontalement selon le déplacement
    private void FlipSprite(float horizontalDirection)
    {
        // Ignore si le SpriteRenderer est absent du joueur
        if (_spriteRenderer == null)
        {
            return;
        }

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

    // Retourne la dernière direction de déplacement valide
    public Vector2 GetFacingDirection() => _lastFacingDirection;

    // Modifie la vitesse de déplacement depuis l'extérieur
    public void SetSpeed(float newSpeed)
    {
        // Met à jour la vitesse avec la nouvelle valeur
        _moveSpeed = newSpeed;
    }
}
