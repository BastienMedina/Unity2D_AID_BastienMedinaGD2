using System.Collections;
using UnityEngine;
using TMPro;

public class CoinsDisplay : MonoBehaviour
{
    [SerializeField] private CoinSystem _coinSystem;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private int _victoryTarget = 3;
    [SerializeField] private float _punchDuration = 0.3f;
    [SerializeField] private float _punchScale = 1.4f;

    private const string CoinsSeparator = " / ";

    private Vector3 _textBaseScale;
    private Coroutine _punchCoroutine;

    private void Awake() // Stocke le scale initial et affiche zéro
    {
        _textBaseScale = _text != null ? _text.transform.localScale : Vector3.one;

        if (_coinSystem == null)
        {
            Debug.LogWarning("[CoinsDisplay] Référence CoinSystem non assignée.", this);
            return;
        }

        UpdateText(0);
    }

    private void OnEnable() // S'abonne à l'événement de collecte
    {
        if (_coinSystem != null)
            _coinSystem.OnCoinCollected.AddListener(OnCoinCollected);
    }

    private void OnDisable() // Désabonne l'écouteur de collecte
    {
        if (_coinSystem != null)
            _coinSystem.OnCoinCollected.RemoveListener(OnCoinCollected);
    }

    private void OnCoinCollected(int totalCollected) // Met à jour le texte et lance le punch
    {
        UpdateText(totalCollected);

        if (_text == null)
            return;

        if (_punchCoroutine != null)
            StopCoroutine(_punchCoroutine);

        _punchCoroutine = StartCoroutine(ScalePunchCoroutine());
    }

    private void UpdateText(int totalCollected) // Compose et affiche la chaîne compteur
    {
        if (_text == null)
            return;

        _text.text = totalCollected + CoinsSeparator + _victoryTarget;
    }

    private IEnumerator ScalePunchCoroutine() // Gonfle puis revient au scale initial
    {
        float elapsed = 0f;
        float half    = _punchDuration * 0.5f;
        Vector3 peak  = _textBaseScale * _punchScale;
        Transform t   = _text.transform;

        while (elapsed < half) // Phase montante
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.Lerp(_textBaseScale, peak, Mathf.Clamp01(elapsed / half));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half) // Phase descendante
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.Lerp(peak, _textBaseScale, Mathf.Clamp01(elapsed / half));
            yield return null;
        }

        t.localScale    = _textBaseScale;
        _punchCoroutine = null;
    }
}
