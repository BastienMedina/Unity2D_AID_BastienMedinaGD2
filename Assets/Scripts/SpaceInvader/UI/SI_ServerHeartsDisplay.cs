using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SI_ServerHeartsDisplay : MonoBehaviour
{
    [SerializeField] private SI_ServerHealth _serverHealth;
    [SerializeField] private GameObject _heartPrefab;
    [SerializeField] private RectTransform _heartsContainer;
    [SerializeField] private float _damagePunchDuration = 0.35f;
    [SerializeField] private float _damagePunchScale = 0.25f;
    [SerializeField] private int _damageBlinkCount = 3;
    [SerializeField] private float _damageBlinkHalfPeriod = 0.07f;

    private static readonly Color ColorFull   = Color.white;
    private static readonly Color ColorEmpty  = new Color(1f, 1f, 1f, 0.15f);
    private static readonly Color ColorDamage = new Color(1f, 0.15f, 0.15f, 1f);

    private readonly List<Image> _heartImages = new List<Image>();
    private int _lastKnownIntegrity = -1;
    private Vector3 _containerBaseScale;
    private Coroutine _feedbackCoroutine;

    private void Awake() // Résout les dépendances et mémorise le scale du container
    {
        if (_serverHealth == null) _serverHealth = FindFirstObjectByType<SI_ServerHealth>();
        if (_serverHealth == null) Debug.LogError("[SI_ServerHeartsDisplay] SI_ServerHealth introuvable.", this);
        _containerBaseScale = _heartsContainer != null ? _heartsContainer.localScale : Vector3.one;
    }

    private void Start() // Construit les coeurs une fois l'état serveur initialisé
    {
        if (_serverHealth == null) return;
        BuildHearts(_serverHealth.GetMaxIntegrity());
        _lastKnownIntegrity = _serverHealth.GetCurrentIntegrity();
        RefreshHearts(_lastKnownIntegrity);
    }

    private void OnEnable()  // S'abonne à l'événement de dégât du serveur
    {
        if (_serverHealth != null) _serverHealth._onServerDamaged.AddListener(OnServerDamaged);
    }

    private void OnDisable() // Se désabonne proprement à la désactivation
    {
        if (_serverHealth != null) _serverHealth._onServerDamaged.RemoveListener(OnServerDamaged);
    }

    private void OnServerDamaged(int remainingIntegrity) // Rafraîchit les coeurs et déclenche le feedback visuel
    {
        _lastKnownIntegrity = remainingIntegrity;
        RefreshHearts(remainingIntegrity);

        if (_heartsContainer != null)
        {
            if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
            _feedbackCoroutine = StartCoroutine(DamageFeedbackCoroutine());
        }
    }

    private void BuildHearts(int maxIntegrity) // Instancie un coeur par point d'intégrité maximum
    {
        if (_heartPrefab == null) { Debug.LogError("[SI_ServerHeartsDisplay] _heartPrefab non assigné.", this); return; }

        foreach (Image img in _heartImages) if (img != null) Destroy(img.gameObject); // Nettoie les anciens coeurs
        _heartImages.Clear();

        for (int i = 0; i < maxIntegrity; i++) // Instancie chaque coeur dans le container
        {
            GameObject heartGO = Instantiate(_heartPrefab, _heartsContainer);
            Image img = heartGO.GetComponent<Image>() ?? heartGO.GetComponentInChildren<Image>();
            if (img != null) { img.color = ColorFull; _heartImages.Add(img); }
        }
    }

    private void RefreshHearts(int currentIntegrity) // Colorie plein/vide selon l'intégrité restante
    {
        for (int i = 0; i < _heartImages.Count; i++)
            if (_heartImages[i] != null)
                _heartImages[i].color = i < currentIntegrity ? ColorFull : ColorEmpty;
    }

    private IEnumerator DamageFeedbackCoroutine() // Punch de scale et clignotements rouges simultanés
    {
        StartCoroutine(ScalePunchCoroutine());

        for (int b = 0; b < _damageBlinkCount; b++) // Alterne couleur dégât et couleur correcte
        {
            SetHeartsColor(ColorDamage);
            yield return new WaitForSeconds(_damageBlinkHalfPeriod);
            ApplyCorrectColors();
            yield return new WaitForSeconds(_damageBlinkHalfPeriod);
        }

        _feedbackCoroutine = null;
    }

    private IEnumerator ScalePunchCoroutine() // Gonfle puis revient au scale de base
    {
        float peakScale = 1f + _damagePunchScale;
        float half      = _damagePunchDuration * 0.5f;
        float elapsed   = 0f;

        while (elapsed < half) // Phase montante du punch
        {
            elapsed += Time.deltaTime;
            _heartsContainer.localScale = _containerBaseScale * Mathf.Lerp(1f, peakScale, Mathf.Clamp01(elapsed / half));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half) // Phase descendante du punch
        {
            elapsed += Time.deltaTime;
            _heartsContainer.localScale = _containerBaseScale * Mathf.Lerp(peakScale, 1f, Mathf.Clamp01(elapsed / half));
            yield return null;
        }

        _heartsContainer.localScale = _containerBaseScale;
    }

    private void SetHeartsColor(Color color) // Applique une couleur unique à tous les coeurs
    {
        foreach (Image img in _heartImages) if (img != null) img.color = color;
    }

    private void ApplyCorrectColors() // Réapplique plein/vide selon l'intégrité mémorisée
    {
        for (int i = 0; i < _heartImages.Count; i++)
            if (_heartImages[i] != null)
                _heartImages[i].color = i < _lastKnownIntegrity ? ColorFull : ColorEmpty;
    }
}
