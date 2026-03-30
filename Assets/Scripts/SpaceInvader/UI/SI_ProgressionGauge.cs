using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Suit le temps de survie et pilote le slider de progression
public class SI_ProgressionGauge : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Références et durée
    // -------------------------------------------------------------------------

    // Slider UI construit en code et mis à jour chaque frame
    private Slider _progressSlider;

    // Durée totale en secondes à survivre pour déclencher la victoire
    [SerializeField] private float _totalDuration = 120f;

    // Gestionnaire de pause pour suspendre le timer si le jeu est en pause
    [SerializeField] private PauseManager _pauseManager;

    // -------------------------------------------------------------------------
    // Apparence du slider construit en code
    // -------------------------------------------------------------------------

    // Position d'ancrage minimale en X du slider dans le Canvas
    [SerializeField] private float _sliderAnchorMinX = 0.1f;

    // Position d'ancrage minimale en Y du slider dans le Canvas
    [SerializeField] private float _sliderAnchorMinY = 0.92f;

    // Position d'ancrage maximale en X du slider dans le Canvas
    [SerializeField] private float _sliderAnchorMaxX = 0.9f;

    // Position d'ancrage maximale en Y du slider dans le Canvas
    [SerializeField] private float _sliderAnchorMaxY = 0.98f;

    // Couleur du fond de la barre de progression
    [SerializeField] private Color _backgroundColor = new Color(0.15f, 0.15f, 0.25f, 1f);

    // Couleur du remplissage de la barre de survie
    [SerializeField] private Color _fillColor = new Color(0.2f, 0.8f, 1f, 1f);

    // Marge latérale du Fill Area en pixels pour éviter le clipping
    [SerializeField] private float _fillAreaMargin = 5f;

    // -------------------------------------------------------------------------
    // Événements publics
    // -------------------------------------------------------------------------

    // Événement déclenché toutes les demi-secondes avec la progression
    [SerializeField] private UnityEvent<float> _onProgressUpdated;

    // Événement déclenché une seule fois à l'atteinte de la victoire
    public UnityEvent OnVictory;

    // -------------------------------------------------------------------------
    // Validation de la durée minimale
    // -------------------------------------------------------------------------

    // Durée minimale acceptable pour la condition de victoire
    [SerializeField] private float _minVictoryDuration = 10f;

    // -------------------------------------------------------------------------
    // État interne du timer
    // -------------------------------------------------------------------------

    // Temps de survie total accumulé depuis le début de la partie
    private float _elapsedTime;

    // Indique si le timer est actif et doit être incrémenté
    private bool _timerActive = true;

    // Indique si la victoire a déjà été déclenchée pour éviter le double
    private bool _finished;

    // Accumulateur interne pour l'intervalle de notification de progression
    private float _progressNotifyAccumulator;

    // Intervalle fixe entre chaque notification de progression en secondes
    private const float ProgressNotifyInterval = 0.5f;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Valide la durée et le PauseManager au plus tôt
    private void Awake()
    {
        // Corrige la durée si la valeur configurée est nulle ou invalide
        if (_totalDuration <= 0f)
        {
            // Avertit que la durée configurée est invalide et applique le minimum
            Debug.LogWarning("[GAUGE] _totalDuration invalide, corrigé au minimum");

            // Force la durée de victoire à la valeur minimale acceptable
            _totalDuration = _minVictoryDuration;
        }

        // Signale si le PauseManager est manquant dans l'inspecteur
        if (_pauseManager == null)
        {
            Debug.LogWarning("[GAUGE] PauseManager non assigné — pause ignorée");
        }
    }

    // Construit le slider après que tous les Awake() sont exécutés
    private void Start()
    {
        // Lance la construction du slider une fois le Canvas disponible
        BuildSlider();
    }

    // Construit le slider de survie dynamiquement dans le Canvas
    private void BuildSlider()
    {
        // Cherche le canvas dans la scène après tous les Awake
        Canvas canvas = FindObjectOfType<Canvas>();

        // Abandonne si aucun Canvas n'existe dans la scène
        if (canvas == null)
        {
            Debug.LogError("[SI] Aucun Canvas trouvé pour le slider !");
            return;
        }

        // Détruit un ancien slider s'il existe déjà
        Transform old = canvas.transform.Find("Slider_Survival");
        if (old != null) Destroy(old.gameObject);

        // Crée le GameObject racine portant le RectTransform
        GameObject sliderGO = new GameObject("Slider_Survival");
        sliderGO.transform.SetParent(canvas.transform, false);

        // Positionne la barre en haut du canvas via les ancres configurées
        RectTransform sliderRt = sliderGO.AddComponent<RectTransform>();
        sliderRt.anchorMin = new Vector2(_sliderAnchorMinX, _sliderAnchorMinY);
        sliderRt.anchorMax = new Vector2(_sliderAnchorMaxX, _sliderAnchorMaxY);
        sliderRt.offsetMin = Vector2.zero;
        sliderRt.offsetMax = Vector2.zero;

        // --- BACKGROUND ---

        // Crée le fond de la barre de progression
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform, false);

        // Étire le fond sur toute la surface du slider parent
        RectTransform bgRt = bgGO.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // Applique la couleur de fond configurée en inspecteur
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = _backgroundColor;

        // --- FILL AREA ---

        // Crée la zone conteneur du remplissage avec marges Unity standard
        GameObject fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);

        // Marges latérales standard Unity pour éviter le clipping du fill
        RectTransform fillAreaRt = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.offsetMin = new Vector2(_fillAreaMargin, 0f);
        fillAreaRt.offsetMax = new Vector2(-_fillAreaMargin, 0f);

        // --- FILL ---

        // Crée l'image de remplissage pilotée par la valeur du slider
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);

        // Étire le fill pour qu'il occupe toute la zone de remplissage
        RectTransform fillRt = fillGO.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.sizeDelta = Vector2.zero;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;

        // Applique la couleur cyan configurée à l'image de remplissage
        Image fillImg = fillGO.AddComponent<Image>();
        fillImg.color = _fillColor;

        // --- SLIDER COMPONENT ---

        // Ajoute Slider après les enfants pour que fillRect soit valide
        Slider slider = sliderGO.AddComponent<Slider>();
        slider.minValue    = 0f;
        slider.maxValue    = 1f;
        slider.value       = 0f;

        // Désactive l'interaction joueur sur la barre de survie
        slider.interactable = false;

        // Direction gauche vers droite obligatoire pour l'affichage
        slider.direction = Slider.Direction.LeftToRight;

        // Assigne le fill — critique pour que le slider s'affiche
        slider.fillRect = fillRt;

        // Stocke la référence pour la mise à jour en Update
        _progressSlider = slider;

        Debug.Log("[SI] Slider_Survival créé avec succès !");
    }

    // Incrémente le timer, pilote le slider et vérifie la victoire
    private void Update()
    {
        // Ignore toute mise à jour si la partie est déjà terminée
        if (_finished)
        {
            return;
        }

        // Ignore si le timer est suspendu manuellement
        if (!_timerActive)
        {
            return;
        }

        // Ignore si le jeu est en pause via le PauseManager
        if (_pauseManager != null && _pauseManager.IsPaused)
        {
            return;
        }

        // Accumule le temps réel écoulé depuis la dernière frame
        _elapsedTime += Time.deltaTime;

        // Clamp le temps écoulé pour ne pas dépasser la durée totale
        _elapsedTime = Mathf.Min(_elapsedTime, _totalDuration);

        // Avance la valeur du slider en fonction du temps écoulé normalisé
        if (_progressSlider != null)
        {
            // Met à jour le slider avec le ratio temps écoulé sur durée totale
            _progressSlider.value = _elapsedTime / _totalDuration;
        }

        // Incrémente l'accumulateur de notification de progression
        _progressNotifyAccumulator += Time.deltaTime;

        // Déclenche la notification si l'intervalle de 0.5s est atteint
        if (_progressNotifyAccumulator >= ProgressNotifyInterval)
        {
            // Réinitialise l'accumulateur au surplus pour rester précis
            _progressNotifyAccumulator -= ProgressNotifyInterval;

            // Notifie les abonnés avec la progression normalisée courante
            _onProgressUpdated?.Invoke(GetProgress());
        }

        // Vérifie si le temps de survie requis est complètement atteint
        if (_elapsedTime >= _totalDuration)
        {
            // Déclenche la séquence de victoire une seule fois
            TriggerVictory();
        }
    }

    // -------------------------------------------------------------------------
    // Victoire
    // -------------------------------------------------------------------------

    // Verrouille la jauge à 1 et déclenche l'événement de victoire
    private void TriggerVictory()
    {
        // Marque la partie comme terminée pour bloquer tout futur incrément
        _finished = true;

        // Désactive le timer pour bloquer tout incrément résiduel post-victoire
        _timerActive = false;

        // Force le slider à sa valeur maximale pour afficher 100 %
        if (_progressSlider != null)
        {
            // Verrouille la valeur du slider à un pour signaler la complétion
            _progressSlider.value = 1f;
        }

        // Notifie les abonnés avec une progression finale à 1
        _onProgressUpdated?.Invoke(1f);

        // Déclenche l'événement de victoire pour les systèmes abonnés
        OnVictory?.Invoke();
    }

    // -------------------------------------------------------------------------
    // API publique
    // -------------------------------------------------------------------------

    /// <summary>Suspend le timer pendant une transition ou à la mort.</summary>
    // Bloque l'incrémentation du timer de survie
    public void PauseTimer()
    {
        // Désactive le flag d'activité pour stopper l'incrémentation
        _timerActive = false;
    }

    /// <summary>Reprend le timer après une transition entre les vagues.</summary>
    // Réactive l'incrémentation du timer de survie
    public void ResumeTimer()
    {
        // Réactive le flag d'activité pour reprendre l'incrémentation
        _timerActive = true;
    }

    /// <summary>Arrête définitivement la progression, par exemple à la mort.</summary>
    // Stoppe la jauge sans déclencher la victoire
    public void StopProgression()
    {
        // Marque la partie comme terminée sans invoquer OnVictory
        _finished = true;
    }

    /// <summary>Retourne la progression normalisée entre 0 et 1.</summary>
    // Calcule le ratio du temps écoulé sur la durée totale
    public float GetProgress()
    {
        // Retourne zéro si la durée totale est invalide ou nulle
        if (_totalDuration <= 0f)
        {
            return 0f;
        }

        // Renvoie le ratio normalisé clampé entre 0 et 1
        return Mathf.Clamp01(_elapsedTime / _totalDuration);
    }
}
