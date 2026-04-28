using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyFeedback : MonoBehaviour
{
    [SerializeField] private float _hitScaleDuration = 0.15f;
    [SerializeField] private float _hitScalePunch = 1.4f;
    [SerializeField] private Color _hitFlashColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private float _shootSquishDuration = 0.12f;
    [SerializeField] private float _shootSquishX = 0.7f;
    [SerializeField] private float _rockAmplitude = 12f;
    [SerializeField] private float _rockSpeed = 8f;

    private SpriteRenderer _spriteRenderer;
    private Vector3 _baseScale;
    private Coroutine _scaleCoroutine;
    private Coroutine _flashCoroutine;
    private float _rockTime;
    private bool _isRocking;
    private float _baseRotationZ;

    private void Awake() // Initialise les références et sauvegarde le scale
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _baseScale      = transform.localScale;
    }

    private void Update() // Applique l'oscillation de rotation si active
    {
        if (!_isRocking)
            return;

        _rockTime += Time.deltaTime * _rockSpeed;
        transform.rotation = Quaternion.Euler(0f, 0f, _baseRotationZ + Mathf.Sin(_rockTime) * _rockAmplitude);
    }

    public void PlayHitFeedback() // Lance le punch scale et le flash rouge
    {
        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(ScalePunch(_hitScalePunch, _hitScaleDuration));

        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(ColorFlash(_hitFlashColor, _hitScaleDuration));
    }

    public void PlayShootFeedback() // Lance l'animation de squish de tir
    {
        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(Squish(_shootSquishX, _shootSquishDuration));
    }

    public void SetMovementRock(bool active, float baseRotationZ = 0f) // Active ou désactive l'oscillation de rotation
    {
        _isRocking     = active;
        _baseRotationZ = baseRotationZ;

        if (!active) // Réinitialise la rotation à l'arrêt
        {
            transform.rotation = Quaternion.Euler(0f, 0f, baseRotationZ);
            _rockTime = 0f;
        }
    }

    public void RefreshBaseScale() // Met à jour le scale de référence
    {
        _baseScale = transform.localScale;
    }

    private IEnumerator ScalePunch(float punchFactor, float duration) // Gonfle puis revient au scale de repos
    {
        float half    = duration * 0.5f;
        float elapsed = 0f;

        while (elapsed < half) // Phase montante
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.LerpUnclamped(_baseScale, _baseScale * punchFactor, Mathf.SmoothStep(0f, 1f, elapsed / half));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half) // Phase descendante
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.LerpUnclamped(_baseScale * punchFactor, _baseScale, Mathf.SmoothStep(0f, 1f, elapsed / half));
            yield return null;
        }

        transform.localScale = _baseScale;
        _scaleCoroutine = null;
    }

    private IEnumerator Squish(float squishX, float duration) // Compresse X et étire Y puis revient
    {
        float squishY    = 1f + (1f - squishX);
        Vector3 squished = new Vector3(_baseScale.x * squishX, _baseScale.y * squishY, _baseScale.z);
        float half       = duration * 0.5f;
        float elapsed    = 0f;

        while (elapsed < half) // Phase de compression
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.LerpUnclamped(_baseScale, squished, Mathf.SmoothStep(0f, 1f, elapsed / half));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half) // Phase de retour
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.LerpUnclamped(squished, _baseScale, Mathf.SmoothStep(0f, 1f, elapsed / half));
            yield return null;
        }

        transform.localScale = _baseScale;
        _scaleCoroutine = null;
    }

    private IEnumerator ColorFlash(Color flashColor, float duration) // Flash couleur puis retour au blanc
    {
        _spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(duration * 0.5f);
        _spriteRenderer.color = Color.white;
        _flashCoroutine = null;
    }
}
