using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FloorTransitionAnimator : MonoBehaviour
{
    private const string FontAssetPath = "Assets/TextMesh Pro/Fonts/Tiny5-Regular SDF.asset";

    public static FloorTransitionAnimator Instance { get; private set; }

    [SerializeField] private float _fadeInDuration = 0.6f;
    [SerializeField] private float _fadeOutDuration = 0.6f;
    [SerializeField] private float _labelHoldDuration = 1.2f;
    [SerializeField] private string _labelPrefix = "ÉTAGE ";
    [SerializeField] private AudioClip _transitionClip;

    private Canvas _canvas;
    private Image _overlay;
    private TextMeshProUGUI _label;
    private bool _isTransitioning = false;

    private void Awake() // Initialise le singleton persistant et construit l'UI
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildOverlayUI();
        SetOverlayAlpha(0f);
        _label.gameObject.SetActive(false);
        _canvas.enabled = false; // Désactivé entre les transitions pour les raycasts
    }

    public void TransitionToScene(string sceneName, int nextFloor) // Lance la transition animée vers la scène
    {
        if (_isTransitioning) return;
        StartCoroutine(TransitionCoroutine(sceneName, nextFloor));
    }

    private IEnumerator TransitionCoroutine(string sceneName, int nextFloor) // Fondu sortant, charge scène, fondu entrant
    {
        _isTransitioning = true;
        AudioManager.Instance?.PlaySFX(_transitionClip);
        _canvas.enabled = true;

        yield return StartCoroutine(Fade(0f, 1f, _fadeOutDuration)); // Fondu sortant

        _label.text = _labelPrefix + nextFloor;
        _label.gameObject.SetActive(true);
        yield return new WaitForSeconds(_labelHoldDuration);

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName);
        yield return loadOp; // Attend la fin du chargement

        _label.gameObject.SetActive(false);
        yield return StartCoroutine(Fade(1f, 0f, _fadeInDuration)); // Fondu entrant

        _canvas.enabled  = false;
        _isTransitioning = false;
    }

    private IEnumerator Fade(float fromAlpha, float toAlpha, float duration) // Interpole l'alpha du panneau noir
    {
        float elapsed = 0f;
        while (elapsed < duration) // Avance frame par frame
        {
            elapsed += Time.unscaledDeltaTime;
            SetOverlayAlpha(Mathf.Lerp(fromAlpha, toAlpha, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }
        SetOverlayAlpha(toAlpha);
    }

    private void SetOverlayAlpha(float alpha) // Applique l'alpha uniquement sur l'overlay noir
    {
        Color c = _overlay.color;
        c.a = alpha;
        _overlay.color = c;
    }

    private void BuildOverlayUI() // Crée canvas, panneau noir et label d'étage
    {
        _canvas              = gameObject.AddComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 999;

        CanvasScaler scaler          = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode           = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution   = new Vector2(1920f, 1080f);
        gameObject.AddComponent<GraphicRaycaster>();

        GameObject overlayGO = new GameObject("Overlay");
        overlayGO.transform.SetParent(transform, false);
        _overlay                   = overlayGO.AddComponent<Image>();
        _overlay.color             = Color.black;
        _overlay.raycastTarget     = true;

        RectTransform overlayRect = _overlay.rectTransform;
        overlayRect.anchorMin     = Vector2.zero;
        overlayRect.anchorMax     = Vector2.one;
        overlayRect.offsetMin     = Vector2.zero;
        overlayRect.offsetMax     = Vector2.zero;

        GameObject labelGO = new GameObject("Label_Floor");
        labelGO.transform.SetParent(overlayGO.transform, false);
        _label           = labelGO.AddComponent<TextMeshProUGUI>();
        _label.alignment = TextAlignmentOptions.Center;
        _label.fontSize  = 72f;
        _label.color     = Color.white;
        _label.fontStyle = FontStyles.Bold;

        TMP_FontAsset tiny5 = LoadFont();
        if (tiny5 != null)
            _label.font = tiny5;
        else
            Debug.LogWarning($"[FloorTransitionAnimator] Font Tiny5 introuvable : {FontAssetPath}");

        RectTransform labelRect    = _label.rectTransform;
        labelRect.anchorMin        = new Vector2(0f, 0.5f);
        labelRect.anchorMax        = new Vector2(1f, 0.5f);
        labelRect.pivot            = new Vector2(0.5f, 0.5f);
        labelRect.sizeDelta        = new Vector2(0f, 120f);
        labelRect.anchoredPosition = Vector2.zero;
    }

    private TMP_FontAsset LoadFont() // Charge la font Tiny5 selon le contexte
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
#else
        return Resources.Load<TMP_FontAsset>("Tiny5-Regular SDF");
#endif
    }
}
