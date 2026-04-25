using System.Collections;
using UnityEngine;

// Gère tous les retours visuels animés des ennemis (dégâts, tir, déplacement)
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyFeedback : MonoBehaviour
{
    // Durée totale de l'animation de scale au dégât (montée + descente)
    [SerializeField] private float _hitScaleDuration = 0.15f;

    // Facteur multiplicateur du scale lors d'un dégât reçu
    [SerializeField] private float _hitScalePunch = 1.4f;

    // Couleur du flash au moment de la réception d'un dégât
    [SerializeField] private Color _hitFlashColor = new Color(1f, 0.2f, 0.2f, 1f);

    // Durée totale de l'animation de squish lors d'un tir
    [SerializeField] private float _shootSquishDuration = 0.12f;

    // Facteur de compression X et d'étirement Y lors du squish de tir
    [SerializeField] private float _shootSquishX = 0.7f;

    // Amplitude de l'oscillation de rotation lors du déplacement du chargeur
    [SerializeField] private float _rockAmplitude = 12f;

    // Vitesse de l'oscillation de rotation (cycles par seconde)
    [SerializeField] private float _rockSpeed = 8f;

    // Référence au SpriteRenderer pour les animations de couleur
    private SpriteRenderer _spriteRenderer;

    // Scale de repos sauvegardé à l'initialisation
    private Vector3 _baseScale;

    // Coroutine de scale en cours pour éviter les conflits
    private Coroutine _scaleCoroutine;

    // Coroutine de flash couleur en cours pour éviter les conflits
    private Coroutine _flashCoroutine;

    // Accumulateur de temps pour l'oscillation sinusoïdale du chargeur
    private float _rockTime = 0f;

    // Indique si l'oscillation de rotation est active cette frame
    private bool _isRocking = false;

    // Rotation de base sauvegardée (avant l'oscillation) sur l'axe Z
    private float _baseRotationZ = 0f;

    // Initialise les références et sauvegarde le scale de repos
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _baseScale = transform.localScale;
    }

    // Met à jour l'oscillation de rotation si elle est active
    private void Update()
    {
        if (!_isRocking)
            return;

        _rockTime += Time.deltaTime * _rockSpeed;
        float angle = _baseRotationZ + Mathf.Sin(_rockTime) * _rockAmplitude;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    // -----------------------------------------------------------------------
    // API publique
    // -----------------------------------------------------------------------

    /// <summary>Joue un punch de scale et un flash rouge lors d'un dégât reçu.</summary>
    public void PlayHitFeedback()
    {
        if (_scaleCoroutine != null)
            StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(ScalePunch(_hitScalePunch, _hitScaleDuration));

        if (_flashCoroutine != null)
            StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(ColorFlash(_hitFlashColor, _hitScaleDuration));
    }

    /// <summary>Joue un squish de scale lors d'un tir du EnemyShooter.</summary>
    public void PlayShootFeedback()
    {
        if (_scaleCoroutine != null)
            StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(Squish(_shootSquishX, _shootSquishDuration));
    }

    /// <summary>Active ou désactive l'oscillation de rotation du EnemyCharger.</summary>
    public void SetMovementRock(bool active, float baseRotationZ = 0f)
    {
        _isRocking = active;
        _baseRotationZ = baseRotationZ;

        if (!active)
        {
            // Réinitialise la rotation vers la base quand le mouvement s'arrête
            transform.rotation = Quaternion.Euler(0f, 0f, baseRotationZ);
            _rockTime = 0f;
        }
    }

    /// <summary>Met à jour le scale de référence (ex. après un changement de prefab).</summary>
    public void RefreshBaseScale()
    {
        _baseScale = transform.localScale;
    }

    // -----------------------------------------------------------------------
    // Coroutines d'animation
    // -----------------------------------------------------------------------

    // Gonfle le scale jusqu'au punch puis revient au scale de repos
    private IEnumerator ScalePunch(float punchFactor, float duration)
    {
        float half = duration * 0.5f;
        float elapsed = 0f;

        // Phase montante : scale de repos → scale punch
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / half;
            transform.localScale = Vector3.LerpUnclamped(_baseScale, _baseScale * punchFactor, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        elapsed = 0f;

        // Phase descendante : scale punch → scale de repos
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / half;
            transform.localScale = Vector3.LerpUnclamped(_baseScale * punchFactor, _baseScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        transform.localScale = _baseScale;
        _scaleCoroutine = null;
    }

    // Compresse X et étire Y puis revient au scale de repos (effet de tir)
    private IEnumerator Squish(float squishX, float duration)
    {
        float squishY = 1f + (1f - squishX);
        Vector3 squishScale = new Vector3(_baseScale.x * squishX, _baseScale.y * squishY, _baseScale.z);
        float half = duration * 0.5f;
        float elapsed = 0f;

        // Phase de compression
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / half;
            transform.localScale = Vector3.LerpUnclamped(_baseScale, squishScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        elapsed = 0f;

        // Phase de retour
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / half;
            transform.localScale = Vector3.LerpUnclamped(squishScale, _baseScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        transform.localScale = _baseScale;
        _scaleCoroutine = null;
    }

    // Passe brièvement à la couleur de flash puis revient au blanc
    private IEnumerator ColorFlash(Color flashColor, float duration)
    {
        _spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(duration * 0.5f);
        _spriteRenderer.color = Color.white;
        _flashCoroutine = null;
    }
}
