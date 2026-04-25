using UnityEngine;

// Change le sprite du joueur selon sa direction de déplacement (8 directions)
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerDirectionalSprite : MonoBehaviour
{
    // -----------------------------------------------------------------------
    // Sprites directionnels assignables depuis l'inspecteur
    // -----------------------------------------------------------------------

    [Header("Sprites directionnels")]
    [SerializeField] private Sprite _spriteNorth;
    [SerializeField] private Sprite _spriteNorthEast;
    [SerializeField] private Sprite _spriteEast;
    [SerializeField] private Sprite _spriteSouthEast;
    [SerializeField] private Sprite _spriteSouth;
    [SerializeField] private Sprite _spriteSouthWest;
    [SerializeField] private Sprite _spriteWest;
    [SerializeField] private Sprite _spriteNorthWest;

    // -----------------------------------------------------------------------
    // Champs privés
    // -----------------------------------------------------------------------

    // Référence au SpriteRenderer du joueur
    private SpriteRenderer _spriteRenderer;

    // Référence au composant de mouvement pour lire la direction
    private PlayerMovement _playerMovement;

    // Dernière direction appliquée pour éviter les mises à jour inutiles
    private Vector2 _lastDirection = Vector2.zero;

    // -----------------------------------------------------------------------
    // Cycle de vie Unity
    // -----------------------------------------------------------------------

    // Initialise les références de composants
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    // Met à jour le sprite selon la direction courante chaque frame
    private void Update()
    {
        Vector2 dir = _playerMovement.GetFacingDirection();

        // Ignore si la direction n'a pas changé depuis la dernière frame
        if (dir == _lastDirection)
            return;

        _lastDirection = dir;
        _spriteRenderer.sprite = GetSpriteForDirection(dir);

        // Le flip n'est plus nécessaire — chaque direction a son propre sprite
        _spriteRenderer.flipX = false;
    }

    // -----------------------------------------------------------------------
    // Logique de sélection du sprite
    // -----------------------------------------------------------------------

    /// <summary>Retourne le sprite correspondant à la direction 2D normalisée.</summary>
    private Sprite GetSpriteForDirection(Vector2 dir)
    {
        // Convertit la direction en angle en degrés (0° = Est, sens trigo)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Normalise l'angle dans [0, 360[
        if (angle < 0f)
            angle += 360f;

        // Sélectionne le sprite selon les 8 secteurs de 45° chacun
        // Nord = 90°, Est = 0°, Sud = 270°, Ouest = 180°
        return angle switch
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
