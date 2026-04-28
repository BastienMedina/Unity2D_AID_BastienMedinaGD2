using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Affiche la barre de coeurs du serveur dans le HUD — indépendante des vies du joueur.
// S'abonne aux événements de SI_ServerHealth pour refléter l'intégrité restante.
public class SI_ServerHeartsDisplay : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Références configurables
    // -------------------------------------------------------------------------

    // Composant santé du serveur à écouter
    [SerializeField] private SI_ServerHealth _serverHealth;

    // Prefab d'une icône de coeur UI (doit contenir un composant Image)
    [SerializeField] private GameObject _heartPrefab;

    // Container horizontal où les coeurs sont instanciés
    [SerializeField] private RectTransform _heartsContainer;

    // -------------------------------------------------------------------------
    // Paramètres de feedback visuel
    // -------------------------------------------------------------------------

    // Durée totale du punch de scale au dégât (secondes)
    [SerializeField] private float _damagePunchDuration = 0.35f;

    // Amplitude du punch de scale (valeur ajoutée à 1)
    [SerializeField] private float _damagePunchScale = 0.25f;

    // Nombre de clignotements rouges au dégât
    [SerializeField] private int _damageBlinkCount = 3;

    // Durée d'un demi-cycle de clignotement (secondes)
    [SerializeField] private float _damageBlinkHalfPeriod = 0.07f;

    // -------------------------------------------------------------------------
    // Couleurs
    // -------------------------------------------------------------------------

    // Couleur d'un coeur plein (intégrité restante)
    private static readonly Color ColorFull   = Color.white;

    // Couleur d'un coeur vide (intégrité perdue) — semi-transparent
    private static readonly Color ColorEmpty  = new Color(1f, 1f, 1f, 0.15f);

    // Couleur de clignotement rouge au dégât
    private static readonly Color ColorDamage = new Color(1f, 0.15f, 0.15f, 1f);

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    // Images des coeurs instanciés, dans l'ordre gauche-droite
    private readonly List<Image> _heartImages = new List<Image>();

    // Dernière valeur d'intégrité connue pour détecter les changements
    private int _lastKnownIntegrity = -1;

    // Scale de référence du container au démarrage
    private Vector3 _containerBaseScale;

    // Coroutine de feedback en cours (évite les chevauchements)
    private Coroutine _feedbackCoroutine;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Résout les dépendances et mémorise le scale de base du container
    private void Awake()
    {
        if (_serverHealth == null)
            _serverHealth = FindFirstObjectByType<SI_ServerHealth>();

        if (_serverHealth == null)
            Debug.LogError("[SI_ServerHeartsDisplay] SI_ServerHealth introuvable dans la scène.", this);

        _containerBaseScale = _heartsContainer != null
            ? _heartsContainer.localScale
            : Vector3.one;
    }

    // Construit les coeurs une fois que SI_ServerHealth a initialisé son état
    private void Start()
    {
        if (_serverHealth == null) return;

        int max = _serverHealth.GetMaxIntegrity();
        BuildHearts(max);

        _lastKnownIntegrity = _serverHealth.GetCurrentIntegrity();
        RefreshHearts(_lastKnownIntegrity);
    }

    // S'abonne à l'événement de dégât du serveur à l'activation
    private void OnEnable()
    {
        if (_serverHealth == null) return;
        _serverHealth._onServerDamaged.AddListener(OnServerDamaged);
    }

    // Se désabonne proprement à la désactivation
    private void OnDisable()
    {
        if (_serverHealth == null) return;
        _serverHealth._onServerDamaged.RemoveListener(OnServerDamaged);
    }

    // -------------------------------------------------------------------------
    // Callbacks événements
    // -------------------------------------------------------------------------

    // Reçoit la nouvelle intégrité depuis SI_ServerHealth._onServerDamaged
    private void OnServerDamaged(int remainingIntegrity)
    {
        _lastKnownIntegrity = remainingIntegrity;
        RefreshHearts(remainingIntegrity);

        // Déclenche le feedback visuel à chaque dégât reçu
        if (_heartsContainer != null)
        {
            if (_feedbackCoroutine != null)
                StopCoroutine(_feedbackCoroutine);

            _feedbackCoroutine = StartCoroutine(DamageFeedbackCoroutine());
        }
    }

    // -------------------------------------------------------------------------
    // Construction et mise à jour de la barre
    // -------------------------------------------------------------------------

    // Instancie autant de coeurs que le maximum d'intégrité configuré
    private void BuildHearts(int maxIntegrity)
    {
        if (_heartPrefab == null)
        {
            Debug.LogError("[SI_ServerHeartsDisplay] _heartPrefab non assigné.", this);
            return;
        }

        // Nettoie les anciens coeurs si la méthode est rappelée
        foreach (Image img in _heartImages)
        {
            if (img != null)
                Destroy(img.gameObject);
        }
        _heartImages.Clear();

        // Instancie un coeur par point d'intégrité maximum
        for (int i = 0; i < maxIntegrity; i++)
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

    // Met à jour la couleur de chaque coeur selon l'intégrité restante
    private void RefreshHearts(int currentIntegrity)
    {
        for (int i = 0; i < _heartImages.Count; i++)
        {
            if (_heartImages[i] == null) continue;
            _heartImages[i].color = i < currentIntegrity ? ColorFull : ColorEmpty;
        }
    }

    // -------------------------------------------------------------------------
    // Feedback visuel — dégât
    // -------------------------------------------------------------------------

    // Punch de scale + clignotements rouges simultanés sur le container
    private IEnumerator DamageFeedbackCoroutine()
    {
        StartCoroutine(ScalePunchCoroutine());

        for (int b = 0; b < _damageBlinkCount; b++)
        {
            SetHeartsColor(ColorDamage);
            yield return new WaitForSeconds(_damageBlinkHalfPeriod);
            ApplyCorrectColors();
            yield return new WaitForSeconds(_damageBlinkHalfPeriod);
        }

        _feedbackCoroutine = null;
    }

    // Gonfle puis revient au scale de base sur _damagePunchDuration secondes
    private IEnumerator ScalePunchCoroutine()
    {
        float elapsed   = 0f;
        float half      = _damagePunchDuration * 0.5f;
        float peakScale = 1f + _damagePunchScale;

        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            _heartsContainer.localScale = _containerBaseScale * Mathf.Lerp(1f, peakScale, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            _heartsContainer.localScale = _containerBaseScale * Mathf.Lerp(peakScale, 1f, t);
            yield return null;
        }

        _heartsContainer.localScale = _containerBaseScale;
    }

    // Applique une couleur unique à tous les coeurs instanciés
    private void SetHeartsColor(Color color)
    {
        foreach (Image img in _heartImages)
        {
            if (img != null)
                img.color = color;
        }
    }

    // Réapplique les couleurs correctes (plein/vide) selon l'intégrité mémorisée
    private void ApplyCorrectColors()
    {
        for (int i = 0; i < _heartImages.Count; i++)
        {
            if (_heartImages[i] != null)
                _heartImages[i].color = i < _lastKnownIntegrity ? ColorFull : ColorEmpty;
        }
    }
}
