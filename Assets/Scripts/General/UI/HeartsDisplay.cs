using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Affiche une barre de coeurs synchronisée avec le LivesManager.
// Instancie un prefab par coeur max et active/désactive les coeurs selon les vies restantes.
public class HeartsDisplay : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Références configurables
    // -------------------------------------------------------------------------

    // Gestionnaire de vies à écouter — auto-résolu si non assigné.
    [SerializeField] private LivesManager _livesManager;

    // Prefab d'un coeur UI (doit contenir un composant Image).
    [SerializeField] private GameObject _heartPrefab;

    // Container horizontal où les coeurs sont instanciés.
    [SerializeField] private RectTransform _heartsContainer;

    // -------------------------------------------------------------------------
    // Paramètres de feedback
    // -------------------------------------------------------------------------

    // Durée totale du punch de scale au dégât (secondes).
    [SerializeField] private float _damagePunchDuration = 0.35f;

    // Amplitude du punch de scale (valeur ajoutée à 1).
    [SerializeField] private float _damagePunchScale = 0.25f;

    // Nombre de clignotements rouges au dégât.
    [SerializeField] private int _damageBlinkCount = 3;

    // Durée d'un demi-cycle de clignotement (secondes).
    [SerializeField] private float _damageBlinkHalfPeriod = 0.07f;

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    // Liste des images de coeurs instanciés, dans l'ordre gauche-droite.
    private readonly List<Image> _heartImages = new List<Image>();

    // Dernière valeur de vies connue pour détecter les pertes.
    private int _lastKnownLives = -1;

    // Coroutine de feedback en cours (évite les chevauchements).
    private Coroutine _feedbackCoroutine;

    // Scale de référence du container au démarrage.
    private Vector3 _containerBaseScale;

    // Couleur d'un coeur plein (vie restante).
    private static readonly Color ColorFull  = Color.white;

    // Couleur d'un coeur vide (vie perdue) — transparent.
    private static readonly Color ColorEmpty = new Color(1f, 1f, 1f, 0.15f);

    // Couleur de clignotement rouge au dégât.
    private static readonly Color ColorDamage = new Color(1f, 0.15f, 0.15f, 1f);

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Résout les dépendances au plus tôt (avant Start).
    private void Awake()
    {
        ResolveManager();
        _containerBaseScale = _heartsContainer != null
            ? _heartsContainer.localScale
            : Vector3.one;
    }

    // Construit la barre de coeurs une fois que tous les Awake() sont terminés,
    // garantissant que LivesManager.Instance est disponible quelle que soit la scène.
    private void Start()
    {
        // Deuxième tentative de résolution si Awake n'a pas réussi
        // (cas où LivesManager.Awake() s'est exécuté après HeartsDisplay.Awake()).
        if (_livesManager == null)
            ResolveManager();

        if (_livesManager == null)
        {
            Debug.LogError("[HeartsDisplay] LivesManager introuvable dans la scène.", this);
            return;
        }

        // Abonnements initiaux — OnEnable sera appelé après Start si le GO est actif,
        // donc on n'abonne PAS ici pour éviter le double abonnement.
        BuildHearts(_livesManager.GetMaxLives());
        _lastKnownLives = _livesManager.GetCurrentLives();
        RefreshHearts(_livesManager.GetCurrentLives());
    }

    // Abonne les mises à jour aux événements du LivesManager (réactivation après désactivation).
    private void OnEnable()
    {
        if (_livesManager == null)
            return;

        _livesManager.OnLivesChanged.AddListener(RefreshHearts);
        _livesManager.OnMaxHealthChanged.AddListener(OnMaxHealthChanged);
    }

    // Désabonne pour éviter les fuites mémoire.
    private void OnDisable()
    {
        if (_livesManager == null)
            return;

        _livesManager.OnLivesChanged.RemoveListener(RefreshHearts);
        _livesManager.OnMaxHealthChanged.RemoveListener(OnMaxHealthChanged);
    }

    // Reconstruit les coeurs et rafraîchit leur état quand le maximum de vies change.
    private void OnMaxHealthChanged(int newMax)
    {
        BuildHearts(newMax);
        _lastKnownLives = _livesManager.GetCurrentLives();
        RefreshHearts(_lastKnownLives);
    }

    // Tente de localiser le LivesManager via le singleton puis via la scène.
    private void ResolveManager()
    {
        if (_livesManager != null)
            return;

        _livesManager = LivesManager.Instance;

        if (_livesManager == null)
            _livesManager = FindFirstObjectByType<LivesManager>();
    }

    // -------------------------------------------------------------------------
    // Méthodes privées
    // -------------------------------------------------------------------------

    // Instancie autant de coeurs que le maximum de vies et les affiche tous pleins.
    private void BuildHearts(int maxLives)
    {
        if (_heartPrefab == null)
        {
            Debug.LogError("[HeartsDisplay] _heartPrefab non assigné — assignez un prefab de coeur dans l'Inspector.", this);
            return;
        }

        // Nettoie les anciens coeurs si la méthode est rappelée
        foreach (Image img in _heartImages)
        {
            if (img != null)
                Destroy(img.gameObject);
        }
        _heartImages.Clear();

        // Instancie un coeur par vie maximum
        for (int i = 0; i < maxLives; i++)
        {
            GameObject heartGO = Instantiate(_heartPrefab, _heartsContainer);
            Image img = heartGO.GetComponent<Image>();

            if (img == null)
                img = heartGO.GetComponentInChildren<Image>();

            if (img != null)
            {
                img.color = ColorFull;
                _heartImages.Add(img);
            }
        }
    }

    // Met à jour la couleur de chaque coeur selon les vies restantes.
    // Déclenche le feedback visuel si les vies ont diminué.
    private void RefreshHearts(int currentLives)
    {
        bool tookDamage = _lastKnownLives > 0 && currentLives < _lastKnownLives;
        _lastKnownLives = currentLives;

        for (int i = 0; i < _heartImages.Count; i++)
        {
            if (_heartImages[i] == null)
                continue;

            // Coeur plein si l'index est inférieur aux vies restantes, vide sinon
            _heartImages[i].color = i < currentLives ? ColorFull : ColorEmpty;
        }

        if (tookDamage && _heartsContainer != null)
        {
            if (_feedbackCoroutine != null)
                StopCoroutine(_feedbackCoroutine);

            _feedbackCoroutine = StartCoroutine(DamageFeedbackCoroutine());
        }
    }

    // -------------------------------------------------------------------------
    // Feedback visuel — dégât
    // -------------------------------------------------------------------------

    // Punch de scale + clignotements rouges simultanés sur le container.
    private IEnumerator DamageFeedbackCoroutine()
    {
        // --- Scale punch ---
        StartCoroutine(ScalePunchCoroutine());

        // --- Clignotements rouges ---
        for (int b = 0; b < _damageBlinkCount; b++)
        {
            SetHeartsColor(ColorDamage);
            yield return new WaitForSeconds(_damageBlinkHalfPeriod);
            ApplyCorrectColors();
            yield return new WaitForSeconds(_damageBlinkHalfPeriod);
        }

        _feedbackCoroutine = null;
    }

    // Gonfle puis revient au scale de base sur _damagePunchDuration secondes.
    private IEnumerator ScalePunchCoroutine()
    {
        float elapsed  = 0f;
        float half     = _damagePunchDuration * 0.5f;
        float peakScale = 1f + _damagePunchScale;

        // Phase montante
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.Clamp01(elapsed / half);
            _heartsContainer.localScale = _containerBaseScale * Mathf.Lerp(1f, peakScale, t);
            yield return null;
        }

        // Phase descendante
        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.Clamp01(elapsed / half);
            _heartsContainer.localScale = _containerBaseScale * Mathf.Lerp(peakScale, 1f, t);
            yield return null;
        }

        _heartsContainer.localScale = _containerBaseScale;
    }

    // Applique une couleur unique à tous les coeurs instanciés.
    private void SetHeartsColor(Color color)
    {
        foreach (Image img in _heartImages)
        {
            if (img != null)
                img.color = color;
        }
    }

    // Réapplique les couleurs correctes (plein/vide) selon les vies connues.
    private void ApplyCorrectColors()
    {
        for (int i = 0; i < _heartImages.Count; i++)
        {
            if (_heartImages[i] != null)
                _heartImages[i].color = i < _lastKnownLives ? ColorFull : ColorEmpty;
        }
    }
}
