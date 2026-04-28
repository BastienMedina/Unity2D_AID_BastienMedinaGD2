using System.Collections;
using UnityEngine;

// Joue un effet d'explosion one-shot à la position donnée puis se détruit.
// Instancié par VirusBase à la mort de chaque ennemi Space Invaders.
[RequireComponent(typeof(SpriteRenderer))]
public class SI_ExplosionEffect : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Paramètres configurables
    // -------------------------------------------------------------------------

    // Durée totale de l'effet d'explosion en secondes
    [SerializeField] private float _duration = 0.45f;

    // Nombre de clignotements du sprite pendant l'explosion
    [SerializeField] private int _blinkCount = 4;

    // Scale de départ de l'explosion (gonflement initial)
    [SerializeField] private float _scaleStart = 0.6f;

    // Scale final de l'explosion (expansion vers zéro)
    [SerializeField] private float _scaleEnd = 1.4f;

    // -------------------------------------------------------------------------
    // Références internes
    // -------------------------------------------------------------------------

    // SpriteRenderer de l'explosion gérant couleur et visibilité
    private SpriteRenderer _spriteRenderer;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        StartCoroutine(PlayExplosion());
    }

    // -------------------------------------------------------------------------
    // Coroutine d'animation
    // -------------------------------------------------------------------------

    // Anime le scale, le clignotement et le fade-out, puis détruit le GO.
    private IEnumerator PlayExplosion()
    {
        if (_spriteRenderer == null)
        {
            Destroy(gameObject);
            yield break;
        }

        float elapsed      = 0f;
        float blinkPeriod  = _blinkCount > 0 ? _duration / (_blinkCount * 2f) : _duration;
        float nextBlink    = blinkPeriod;
        bool  visible      = true;

        // Démarre à la taille réduite
        transform.localScale = Vector3.one * _scaleStart;
        _spriteRenderer.color = Color.white;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);

            // Expansion progressive du scale
            float scale = Mathf.Lerp(_scaleStart, _scaleEnd, t);
            transform.localScale = Vector3.one * scale;

            // Fade-out en fin d'effet (derniers 40%)
            if (t > 0.6f)
            {
                float fadeT = (t - 0.6f) / 0.4f;
                _spriteRenderer.color = new Color(1f, 1f, 1f, 1f - fadeT);
            }

            // Clignotement pendant la première moitié
            if (elapsed >= nextBlink && t < 0.6f)
            {
                visible     = !visible;
                nextBlink  += blinkPeriod;
                _spriteRenderer.enabled = visible;
            }

            yield return null;
        }

        // Garantit la visibilité réactivée avant de se détruire
        _spriteRenderer.enabled = true;
        Destroy(gameObject);
    }
}
