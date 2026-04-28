using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerDirectionalSprite : MonoBehaviour
{
    [Header("Sprites directionnels")]
    [SerializeField] private Sprite _spriteNorth;
    [SerializeField] private Sprite _spriteNorthEast;
    [SerializeField] private Sprite _spriteEast;
    [SerializeField] private Sprite _spriteSouthEast;
    [SerializeField] private Sprite _spriteSouth;
    [SerializeField] private Sprite _spriteSouthWest;
    [SerializeField] private Sprite _spriteWest;
    [SerializeField] private Sprite _spriteNorthWest;

    private SpriteRenderer _spriteRenderer;
    private PlayerMovement _playerMovement;
    private Vector2 _lastDirection = Vector2.zero;

    private void Awake() // Initialise les références de composants
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update() // Met à jour le sprite selon la direction courante
    {
        Vector2 dir = _playerMovement.GetFacingDirection();

        if (dir == _lastDirection) // Ignore si la direction n'a pas changé
            return;

        _lastDirection          = dir;
        _spriteRenderer.sprite  = GetSpriteForDirection(dir);
        _spriteRenderer.flipX   = false;
    }

    private Sprite GetSpriteForDirection(Vector2 dir) // Sélectionne le sprite selon les 8 directions
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0f) angle += 360f; // Normalise dans [0, 360[

        return angle switch // Sélectionne selon les 8 secteurs de 45°
        {
            >= 337.5f or < 22.5f   => _spriteEast,
            >= 22.5f  and < 67.5f  => _spriteNorthEast,
            >= 67.5f  and < 112.5f => _spriteNorth,
            >= 112.5f and < 157.5f => _spriteNorthWest,
            >= 157.5f and < 202.5f => _spriteWest,
            >= 202.5f and < 247.5f => _spriteSouthWest,
            >= 247.5f and < 292.5f => _spriteSouth,
            _                      => _spriteSouthEast
        };
    }
}
