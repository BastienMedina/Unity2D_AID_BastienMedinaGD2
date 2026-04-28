using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerFeedback : MonoBehaviour
{
    [Header("Idle")]
    [SerializeField] private float _breathAmplitude = 0.04f;
    [SerializeField] private float _breathSpeed = 1.2f;

    [Header("Marche")]
    [SerializeField] private float _walkBobAmplitude = 0.06f;
    [SerializeField] private float _walkBobSpeed = 12f;

    [Header("Dégâts")]
    [SerializeField] private float _hitScalePunch = 1.35f;
    [SerializeField] private float _hitDuration = 0.2f;
    [SerializeField] private Color _hitFlashColor = new Color(1f, 0.15f, 0.15f, 1f);
    [SerializeField] private int _hitFlashCount = 4;
    [SerializeField] private float _hitInvincibilityDuration = 0.8f;

    [Header("Mort")]
    [SerializeField] private float _deathFadeDuration = 0.5f;

    private SpriteRenderer _spriteRenderer;
    private PlayerMovement _playerMovement;
    private Vector3 _baseScale;
    private float _bobTime = 0f;
    private Coroutine _scaleCoroutine;
    private Coroutine _flashCoroutine;
    private bool _isInvincible = false;
    private int _previousLives = -1;

    private void Awake() // Initialise références et sauvegarde le scale de base
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _playerMovement = GetComponent<PlayerMovement>();
        _baseScale      = transform.localScale;
    }

    private void Start() // S'abonne aux événements du LivesManager
    {
        if (LivesManager.Instance != null)
        {
            _previousLives = LivesManager.Instance.GetCurrentLives();
            LivesManager.Instance.OnLivesChanged.AddListener(OnLivesChanged);
            LivesManager.Instance.OnDeath.AddListener(OnDeath);
        }
    }

    private void OnDestroy() // Se désabonne proprement à la destruction
    {
        if (LivesManager.Instance != null)
        {
            LivesManager.Instance.OnLivesChanged.RemoveListener(OnLivesChanged);
            LivesManager.Instance.OnDeath.RemoveListener(OnDeath);
        }
    }

    private void Update() // Applique bob de marche ou idle selon l'état
    {
        if (_scaleCoroutine != null) // Ignore si une coroutine de scale est active
            return;

        bool isMoving = _playerMovement.GetFacingDirection() != Vector2.zero && _playerMovement.IsMoving();

        if (isMoving)
            ApplyWalkBob();
        else
            ApplyIdleBreath();
    }

    private void ApplyWalkBob() // Squish vertical rythmé pendant la marche
    {
        _bobTime += Time.deltaTime * _walkBobSpeed;
        float sin    = Mathf.Sin(_bobTime);
        float scaleY = _baseScale.y * (1f + sin * _walkBobAmplitude);
        float scaleX = _baseScale.x * (1f - sin * _walkBobAmplitude * 0.5f);
        transform.localScale = new Vector3(scaleX, scaleY, _baseScale.z);
    }

    private void ApplyIdleBreath() // Légère pulsation de scale en idle
    {
        _bobTime += Time.deltaTime * _breathSpeed;
        float breath = 1f + Mathf.Sin(_bobTime) * _breathAmplitude;
        transform.localScale = _baseScale * breath;
    }

    private void OnLivesChanged(int newLives) // Joue feedback seulement en cas de dégât
    {
        bool tookDamage = _previousLives >= 0 && newLives < _previousLives;
        _previousLives  = newLives;

        if (!tookDamage || _isInvincible) // Ignore soins et invincibilité en cours
            return;

        PlayHitFeedback();
    }

    private void OnDeath() // Stoppe les coroutines et lance le fondu de mort
    {
        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        StartCoroutine(DeathFade());
    }

    private void PlayHitFeedback() // Lance punch de scale et flashs d'invincibilité
    {
        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(ScalePunch());

        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(InvincibilityFlash());
    }

    private IEnumerator ScalePunch() // Gonfle le scale puis revient à la normale
    {
        float half       = _hitDuration * 0.5f;
        float elapsed    = 0f;
        Vector3 punchScale = _baseScale * _hitScalePunch;

        while (elapsed < half) // Phase montante
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.LerpUnclamped(_baseScale, punchScale, Mathf.SmoothStep(0f, 1f, elapsed / half));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half) // Phase descendante
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.LerpUnclamped(punchScale, _baseScale, Mathf.SmoothStep(0f, 1f, elapsed / half));
            yield return null;
        }

        transform.localScale = _baseScale;
        _scaleCoroutine      = null;
    }

    private IEnumerator InvincibilityFlash() // Flash rouge alterné pendant l'invincibilité
    {
        _isInvincible    = true;
        float interval   = _hitInvincibilityDuration / (_hitFlashCount * 2f);

        for (int i = 0; i < _hitFlashCount; i++) // Alterne couleur flash et blanche
        {
            _spriteRenderer.color = _hitFlashColor;
            yield return new WaitForSeconds(interval);
            _spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(interval);
        }

        _spriteRenderer.color = Color.white;
        _isInvincible         = false;
        _flashCoroutine       = null;
    }

    private IEnumerator DeathFade() // Fondu d'alpha et scale vers zéro à la mort
    {
        float elapsed    = 0f;
        Color startColor = _spriteRenderer.color;

        while (elapsed < _deathFadeDuration) // Interpole alpha et scale pendant le fondu
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _deathFadeDuration;
            _spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1f, 0f, t));
            transform.localScale  = Vector3.LerpUnclamped(_baseScale, _baseScale * 1.3f, t);
            yield return null;
        }
    }
}
