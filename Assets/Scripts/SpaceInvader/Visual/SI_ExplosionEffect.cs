using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SI_ExplosionEffect : MonoBehaviour
{
    [SerializeField] private float _duration   = 0.45f;
    [SerializeField] private int   _blinkCount = 4;
    [SerializeField] private float _scaleStart = 0.6f;
    [SerializeField] private float _scaleEnd   = 1.4f;

    private SpriteRenderer _spriteRenderer;

    private void Awake() => _spriteRenderer = GetComponent<SpriteRenderer>(); // Récupère le SpriteRenderer de l'explosion

    private void Start() => StartCoroutine(PlayExplosion()); // Démarre l'animation one-shot

    private IEnumerator PlayExplosion() // Anime scale, clignotement et fade-out puis se détruit
    {
        if (_spriteRenderer == null) { Destroy(gameObject); yield break; }

        float elapsed     = 0f;
        float blinkPeriod = _blinkCount > 0 ? _duration / (_blinkCount * 2f) : _duration;
        float nextBlink   = blinkPeriod;
        bool  visible     = true;

        transform.localScale  = Vector3.one * _scaleStart;
        _spriteRenderer.color = Color.white;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.Clamp01(elapsed / _duration);

            transform.localScale = Vector3.one * Mathf.Lerp(_scaleStart, _scaleEnd, t); // Expansion progressive

            if (t > 0.6f) // Fade-out sur les 40% finaux
            {
                float fadeT = (t - 0.6f) / 0.4f;
                _spriteRenderer.color = new Color(1f, 1f, 1f, 1f - fadeT);
            }

            if (elapsed >= nextBlink && t < 0.6f) // Clignotement pendant la première moitié
            {
                visible                 = !visible;
                nextBlink              += blinkPeriod;
                _spriteRenderer.enabled = visible;
            }

            yield return null;
        }

        _spriteRenderer.enabled = true;
        Destroy(gameObject);
    }
}
