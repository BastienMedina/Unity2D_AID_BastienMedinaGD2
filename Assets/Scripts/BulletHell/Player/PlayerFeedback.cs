using System.Collections;
using UnityEngine;

// Gère tous les retours visuels animés du joueur (marche, dégâts, idle, mort)
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerFeedback : MonoBehaviour
{
    // -----------------------------------------------------------------------
    // Paramètres — Idle breath
    // -----------------------------------------------------------------------

    [Header("Idle")]
    // Amplitude du scale pulsant en idle (valeur ajoutée au scale de base)
    [SerializeField] private float _breathAmplitude = 0.04f;

    // Vitesse de la pulsation idle en cycles par seconde
    [SerializeField] private float _breathSpeed = 1.2f;

    // -----------------------------------------------------------------------
    // Paramètres — Bob de marche
    // -----------------------------------------------------------------------

    [Header("Marche")]
    // Amplitude du squish vertical pendant la marche
    [SerializeField] private float _walkBobAmplitude = 0.06f;

    // Vitesse du bob de marche en cycles par seconde (lié à la vitesse de déplacement)
    [SerializeField] private float _walkBobSpeed = 12f;

    // -----------------------------------------------------------------------
    // Paramètres — Dégâts
    // -----------------------------------------------------------------------

    [Header("Dégâts")]
    // Facteur multiplicateur du scale lors d'un dégât reçu
    [SerializeField] private float _hitScalePunch = 1.35f;

    // Durée totale de l'animation de scale au dégât
    [SerializeField] private float _hitDuration = 0.2f;

    // Couleur du flash lors d'un dégât
    [SerializeField] private Color _hitFlashColor = new Color(1f, 0.15f, 0.15f, 1f);

    // Nombre de flashs pendant l'invincibilité post-dégât
    [SerializeField] private int _hitFlashCount = 4;

    // Durée totale de l'invincibilité visuelle post-dégât
    [SerializeField] private float _hitInvincibilityDuration = 0.8f;

    // -----------------------------------------------------------------------
    // Paramètres — Mort
    // -----------------------------------------------------------------------

    [Header("Mort")]
    // Durée du fondu de disparition à la mort
    [SerializeField] private float _deathFadeDuration = 0.5f;

    // -----------------------------------------------------------------------
    // Références privées
    // -----------------------------------------------------------------------

    // SpriteRenderer du joueur pour les animations de couleur
    private SpriteRenderer _spriteRenderer;

    // Composant de mouvement pour détecter si le joueur se déplace
    private PlayerMovement _playerMovement;

    // Scale de repos sauvegardé à l'initialisation
    private Vector3 _baseScale;

    // Accumulateur de temps pour le bob de marche et l'idle
    private float _bobTime = 0f;

    // Coroutine de scale en cours pour éviter les conflits
    private Coroutine _scaleCoroutine;

    // Coroutine de flash en cours pour éviter les conflits
    private Coroutine _flashCoroutine;

    // Indique si le joueur est en état d'invincibilité post-dégât
    private bool _isInvincible = false;

    // Mémorise le dernier total de vies connu pour détecter une perte de vie
    private int _previousLives = -1;

    // -----------------------------------------------------------------------
    // Cycle de vie Unity
    // -----------------------------------------------------------------------

    // Initialise les références et s'abonne aux événements de dégâts
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _playerMovement = GetComponent<PlayerMovement>();
        _baseScale = transform.localScale;
    }

    // S'abonne aux événements du LivesManager une fois la scène initialisée
    private void Start()
    {
        if (LivesManager.Instance != null)
        {
            // Mémorise les vies initiales avant tout événement pour la comparaison
            _previousLives = LivesManager.Instance.GetCurrentLives();
            LivesManager.Instance.OnLivesChanged.AddListener(OnLivesChanged);
            LivesManager.Instance.OnDeath.AddListener(OnDeath);
        }
    }

    // Se désabonne proprement à la destruction
    private void OnDestroy()
    {
        if (LivesManager.Instance != null)
        {
            LivesManager.Instance.OnLivesChanged.RemoveListener(OnLivesChanged);
            LivesManager.Instance.OnDeath.RemoveListener(OnDeath);
        }
    }

    // Applique le bob de marche ou l'idle selon l'état de déplacement
    private void Update()
    {
        // Ignore les animations de scale si une coroutine est en cours
        if (_scaleCoroutine != null)
            return;

        bool isMoving = _playerMovement.GetFacingDirection() != Vector2.zero
                        && _playerMovement.IsMoving();

        if (isMoving)
            ApplyWalkBob();
        else
            ApplyIdleBreath();
    }

    // -----------------------------------------------------------------------
    // Animations continues
    // -----------------------------------------------------------------------

    // Squish vertical rythmé pendant la marche
    private void ApplyWalkBob()
    {
        _bobTime += Time.deltaTime * _walkBobSpeed;
        float sin = Mathf.Sin(_bobTime);

        // Squish : quand sin est positif, étire Y et compresse X et vice-versa
        float scaleY = _baseScale.y * (1f + sin * _walkBobAmplitude);
        float scaleX = _baseScale.x * (1f - sin * _walkBobAmplitude * 0.5f);
        transform.localScale = new Vector3(scaleX, scaleY, _baseScale.z);
    }

    // Légère pulsation de scale en idle
    private void ApplyIdleBreath()
    {
        _bobTime += Time.deltaTime * _breathSpeed;
        float breath = 1f + Mathf.Sin(_bobTime) * _breathAmplitude;
        transform.localScale = _baseScale * breath;
    }

    // -----------------------------------------------------------------------
    // Callbacks événements
    // -----------------------------------------------------------------------

    // Déclenché par LivesManager.OnLivesChanged — joue le feedback uniquement en cas de dégât
    private void OnLivesChanged(int newLives)
    {
        // Détecte une perte de vie en comparant avec la valeur précédente
        bool tookDamage = _previousLives >= 0 && newLives < _previousLives;
        _previousLives = newLives;

        // Ignore les soins, les augmentations de max et les initialisations
        if (!tookDamage) return;

        // Ignore si le flash d'invincibilité est déjà en cours
        if (_isInvincible) return;

        PlayHitFeedback();
    }

    // Déclenché par LivesManager.OnDeath — joue l'animation de mort
    private void OnDeath()
    {
        // Stoppe toutes les coroutines en cours avant la mort
        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);

        StartCoroutine(DeathFade());
    }

    // -----------------------------------------------------------------------
    // Feedback de dégât
    // -----------------------------------------------------------------------

    // Lance le punch de scale et les flashs d'invincibilité
    private void PlayHitFeedback()
    {
        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(ScalePunch());

        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(InvincibilityFlash());
    }

    // -----------------------------------------------------------------------
    // Coroutines
    // -----------------------------------------------------------------------

    // Gonfle le scale au punch puis revient au scale de repos
    private IEnumerator ScalePunch()
    {
        float half = _hitDuration * 0.5f;
        float elapsed = 0f;
        Vector3 punchScale = _baseScale * _hitScalePunch;

        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / half);
            transform.localScale = Vector3.LerpUnclamped(_baseScale, punchScale, t);
            yield return null;
        }

        elapsed = 0f;

        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / half);
            transform.localScale = Vector3.LerpUnclamped(punchScale, _baseScale, t);
            yield return null;
        }

        transform.localScale = _baseScale;
        _scaleCoroutine = null;
    }

    // Flash rouge alterné pendant la durée d'invincibilité
    private IEnumerator InvincibilityFlash()
    {
        _isInvincible = true;
        float interval = _hitInvincibilityDuration / (_hitFlashCount * 2f);

        for (int i = 0; i < _hitFlashCount; i++)
        {
            _spriteRenderer.color = _hitFlashColor;
            yield return new WaitForSeconds(interval);
            _spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(interval);
        }

        _spriteRenderer.color = Color.white;
        _isInvincible = false;
        _flashCoroutine = null;
    }

    // Fondu d'alpha vers zéro à la mort
    private IEnumerator DeathFade()
    {
        float elapsed = 0f;
        Color startColor = _spriteRenderer.color;

        while (elapsed < _deathFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _deathFadeDuration;
            _spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b,
                Mathf.Lerp(1f, 0f, t));
            transform.localScale = Vector3.LerpUnclamped(_baseScale, _baseScale * 1.3f, t);
            yield return null;
        }
    }
}
