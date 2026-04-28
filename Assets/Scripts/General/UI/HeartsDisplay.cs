using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartsDisplay : MonoBehaviour
{
    [SerializeField] private LivesManager _livesManager;
    [SerializeField] private GameObject _heartPrefab;
    [SerializeField] private RectTransform _heartsContainer;

    [SerializeField] private float _damagePunchDuration  = 0.35f;
    [SerializeField] private float _damagePunchScale     = 0.25f;
    [SerializeField] private int   _damageBlinkCount     = 3;
    [SerializeField] private float _damageBlinkHalfPeriod = 0.07f;

    private readonly List<Image> _heartImages = new List<Image>();
    private int _lastKnownLives               = -1;
    private Coroutine _feedbackCoroutine;
    private Vector3 _containerBaseScale;

    private static readonly Color ColorFull   = Color.white;
    private static readonly Color ColorEmpty  = new Color(1f, 1f, 1f, 0.15f);
    private static readonly Color ColorDamage = new Color(1f, 0.15f, 0.15f, 1f);

    private void Awake() // Résout LivesManager et mémorise le scale du container
    {
        ResolveManager();
        _containerBaseScale = _heartsContainer != null ? _heartsContainer.localScale : Vector3.one;
    }

    private void Start() // Construit les coeurs après tous les Awake
    {
        if (_livesManager == null) ResolveManager(); // Deuxième tentative si Awake a échoué

        if (_livesManager == null) return;

        BuildHearts(_livesManager.GetMaxLives());
        _lastKnownLives = _livesManager.GetCurrentLives();
        RefreshHearts(_livesManager.GetCurrentLives());
    }

    private void OnEnable() // Abonne les mises à jour aux événements du LivesManager
    {
        if (_livesManager == null) return;
        _livesManager.OnLivesChanged.AddListener(RefreshHearts);
        _livesManager.OnMaxHealthChanged.AddListener(OnMaxHealthChanged);
    }

    private void OnDisable() // Désabonne pour éviter les fuites mémoire
    {
        if (_livesManager == null) return;
        _livesManager.OnLivesChanged.RemoveListener(RefreshHearts);
        _livesManager.OnMaxHealthChanged.RemoveListener(OnMaxHealthChanged);
    }

    private void OnMaxHealthChanged(int newMax) // Reconstruit et rafraîchit les coeurs au nouveau max
    {
        BuildHearts(newMax);
        _lastKnownLives = _livesManager.GetCurrentLives();
        RefreshHearts(_lastKnownLives);
    }

    private void ResolveManager() // Tente de localiser LivesManager via singleton puis scène
    {
        if (_livesManager != null) return;
        _livesManager = LivesManager.Instance ?? FindFirstObjectByType<LivesManager>();
    }

    private void BuildHearts(int maxLives) // Instancie un coeur par vie maximum après nettoyage
    {
        if (_heartPrefab == null) return;

        foreach (Image img in _heartImages) { if (img != null) Destroy(img.gameObject); }
        _heartImages.Clear();

        for (int i = 0; i < maxLives; i++) // Instancie chaque coeur dans le container
        {
            GameObject heartGO = Instantiate(_heartPrefab, _heartsContainer);
            Image img          = heartGO.GetComponent<Image>() ?? heartGO.GetComponentInChildren<Image>();
            if (img == null) continue;
            img.color = ColorFull;
            _heartImages.Add(img);
        }
    }

    private void RefreshHearts(int currentLives) // Met à jour les couleurs et déclenche le feedback si dégât
    {
        bool tookDamage = _lastKnownLives > 0 && currentLives < _lastKnownLives;
        _lastKnownLives = currentLives;

        for (int i = 0; i < _heartImages.Count; i++) // Plein si index < vies, vide sinon
        {
            if (_heartImages[i] == null) continue;
            _heartImages[i].color = i < currentLives ? ColorFull : ColorEmpty;
        }

        if (tookDamage && _heartsContainer != null) // Déclenche le feedback visuel en cas de dégât
        {
            if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
            _feedbackCoroutine = StartCoroutine(DamageFeedbackCoroutine());
        }
    }

    private IEnumerator DamageFeedbackCoroutine() // Punch de scale et clignotements rouges simultanés
    {
        StartCoroutine(ScalePunchCoroutine());

        for (int b = 0; b < _damageBlinkCount; b++) // Alterne rouge et couleurs correctes
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
        float elapsed   = 0f;
        float half      = _damagePunchDuration * 0.5f;
        float peakScale = 1f + _damagePunchScale;

        while (elapsed < half) // Phase montante
        {
            elapsed += Time.deltaTime;
            _heartsContainer.localScale = _containerBaseScale * Mathf.Lerp(1f, peakScale, Mathf.Clamp01(elapsed / half));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half) // Phase descendante
        {
            elapsed += Time.deltaTime;
            _heartsContainer.localScale = _containerBaseScale * Mathf.Lerp(peakScale, 1f, Mathf.Clamp01(elapsed / half));
            yield return null;
        }

        _heartsContainer.localScale = _containerBaseScale;
    }

    private void SetHeartsColor(Color color) // Applique une couleur uniforme à tous les coeurs
    {
        foreach (Image img in _heartImages) { if (img != null) img.color = color; }
    }

    private void ApplyCorrectColors() // Réapplique plein/vide selon les vies connues
    {
        for (int i = 0; i < _heartImages.Count; i++)
        {
            if (_heartImages[i] != null)
                _heartImages[i].color = i < _lastKnownLives ? ColorFull : ColorEmpty;
        }
    }
}
