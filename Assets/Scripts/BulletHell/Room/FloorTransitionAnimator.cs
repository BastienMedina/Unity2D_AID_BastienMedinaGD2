using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Singleton persistant gérant l'animation de transition entre les étages
public class FloorTransitionAnimator : MonoBehaviour
{
    // Instance globale accessible depuis n'importe quelle scène
    public static FloorTransitionAnimator Instance { get; private set; }

    // Durée du fondu entrant (noir → jeu) en secondes
    [SerializeField] private float _fadeInDuration = 0.6f;

    // Durée du fondu sortant (jeu → noir) en secondes
    [SerializeField] private float _fadeOutDuration = 0.6f;

    // Durée d'affichage du label d'étage en plein noir en secondes
    [SerializeField] private float _labelHoldDuration = 1.2f;

    // Préfixe affiché avant le numéro d'étage dans le label
    [SerializeField] private string _labelPrefix = "ÉTAGE ";

    // Son joué au début de la transition entre étages
    [SerializeField] private AudioClip _transitionClip;

    // Canvas overlay placé au-dessus de tout le rendu
    private Canvas _canvas;

    // Panneau noir couvrant tout l'écran
    private Image _overlay;

    // Texte d'étage affiché au centre pendant le plein noir
    private TextMeshProUGUI _label;

    // Empêche un double déclenchement de transition
    private bool _isTransitioning = false;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildOverlayUI();

        SetOverlayAlpha(0f);
        _label.gameObject.SetActive(false);

        // Désactive le Canvas pour ne pas bloquer les raycasts UI entre les transitions
        _canvas.enabled = false;
    }

    // -------------------------------------------------------------------------
    // API publique
    // -------------------------------------------------------------------------

    /// <summary>Lance la transition animée puis charge la scène cible.</summary>
    public void TransitionToScene(string sceneName, int nextFloor)
    {
        if (_isTransitioning)
            return;

        StartCoroutine(TransitionCoroutine(sceneName, nextFloor));
    }

    // -------------------------------------------------------------------------
    // Coroutine principale
    // -------------------------------------------------------------------------

    private IEnumerator TransitionCoroutine(string sceneName, int nextFloor)
    {
        _isTransitioning = true;

        AudioManager.Instance?.PlaySFX(_transitionClip);

        // Active le Canvas uniquement pendant la transition
        _canvas.enabled = true;

        // Phase 1 : fondu sortant — le jeu disparaît sous le noir
        yield return StartCoroutine(Fade(0f, 1f, _fadeOutDuration));

        // Affiche le label d'étage en plein noir
        _label.text = _labelPrefix + nextFloor;
        _label.gameObject.SetActive(true);

        // Phase 2 : maintien — le joueur lit l'étage
        yield return new WaitForSeconds(_labelHoldDuration);

        // Charge la scène pendant que l'écran est noir
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName);
        yield return loadOp;

        // Cache le label avant le fondu entrant
        _label.gameObject.SetActive(false);

        // Phase 3 : fondu entrant — la nouvelle scène apparaît
        yield return StartCoroutine(Fade(1f, 0f, _fadeInDuration));

        // Désactive le Canvas pour libérer les raycasts UI
        _canvas.enabled = false;

        _isTransitioning = false;
    }

    // -------------------------------------------------------------------------
    // Utilitaires
    // -------------------------------------------------------------------------

    // Interpole l'alpha de l'overlay entre deux valeurs sur une durée donnée
    private IEnumerator Fade(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetOverlayAlpha(Mathf.Lerp(fromAlpha, toAlpha, t));
            yield return null;
        }

        SetOverlayAlpha(toAlpha);
    }

    // Applique l'alpha sur le panneau noir uniquement (label non affecté)
    private void SetOverlayAlpha(float alpha)
    {
        Color c = _overlay.color;
        c.a = alpha;
        _overlay.color = c;
    }

    // -------------------------------------------------------------------------
    // Construction du Canvas overlay par code
    // -------------------------------------------------------------------------

    // Crée le Canvas, le panneau noir et le label d'étage au démarrage
    private void BuildOverlayUI()
    {
        // Canvas en Screen Space - Overlay au-dessus de tout
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 999;

        // CanvasScaler pour l'indépendance de résolution
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        gameObject.AddComponent<GraphicRaycaster>();

        // Panneau noir couvrant tout l'écran
        GameObject overlayGO = new GameObject("Overlay");
        overlayGO.transform.SetParent(transform, false);

        _overlay = overlayGO.AddComponent<Image>();
        _overlay.color = Color.black;
        _overlay.raycastTarget = true;

        RectTransform overlayRect = _overlay.rectTransform;
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        // Label d'étage centré sur le panneau noir
        GameObject labelGO = new GameObject("Label_Floor");
        labelGO.transform.SetParent(overlayGO.transform, false);

        _label = labelGO.AddComponent<TextMeshProUGUI>();
        _label.alignment = TextAlignmentOptions.Center;
        _label.fontSize = 72f;
        _label.color = Color.white;
        _label.fontStyle = FontStyles.Bold;

        RectTransform labelRect = _label.rectTransform;
        labelRect.anchorMin = new Vector2(0f, 0.5f);
        labelRect.anchorMax = new Vector2(1f, 0.5f);
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.sizeDelta = new Vector2(0f, 120f);
        labelRect.anchoredPosition = Vector2.zero;
    }
}
