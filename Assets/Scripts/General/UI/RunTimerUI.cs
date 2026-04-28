using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Affiche le timer de run en bas de l'écran.
// Construit son propre Canvas en overlay par code — aucun prefab requis.
// Doit être placé sur un GameObject persistant (ex. GO_RunTimerUI avec DontDestroyOnLoad).
public class RunTimerUI : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Paramètres configurables
    // -------------------------------------------------------------------------

    // Taille de police du timer
    [SerializeField] private float _fontSize = 40f;

    // Couleur du texte par défaut
    [SerializeField] private Color _textColor = Color.white;

    // Chemin de la font TMP SDF Tiny5
    private const string FontAssetPath = "Assets/TextMesh Pro/Fonts/Tiny5-Regular SDF.asset";

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    private TextMeshProUGUI _label;
    private Canvas          _canvas;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        BuildCanvas();
    }

    private void OnEnable()
    {
        if (RunTimerManager.Instance != null)
            RunTimerManager.Instance.OnTick.AddListener(OnTick);
    }

    private void OnDisable()
    {
        if (RunTimerManager.Instance != null)
            RunTimerManager.Instance.OnTick.RemoveListener(OnTick);
    }

    private void Start()
    {
        // Souscription après que RunTimerManager.Awake() a eu lieu
        if (RunTimerManager.Instance != null)
        {
            RunTimerManager.Instance.OnTick.AddListener(OnTick);
            RunTimerManager.Instance.OnRunFinished.AddListener(OnRunFinished);
        }

        UpdateLabel(0f);
        SetVisible(false);
    }

    // -------------------------------------------------------------------------
    // API publique
    // -------------------------------------------------------------------------

    /// <summary>Affiche ou cache le timer.</summary>
    public void SetVisible(bool visible)
    {
        if (_canvas != null)
            _canvas.gameObject.SetActive(visible);
    }

    // -------------------------------------------------------------------------
    // Callbacks
    // -------------------------------------------------------------------------

    private void OnTick(float elapsed)
    {
        SetVisible(true);
        UpdateLabel(elapsed);
    }

    private void OnRunFinished(float elapsed, bool isNewBest)
    {
        UpdateLabel(elapsed);
    }

    // -------------------------------------------------------------------------
    // Construction du Canvas et du label
    // -------------------------------------------------------------------------

    private void BuildCanvas()
    {
        // Canvas en overlay, persistant
        GameObject canvasGO = new GameObject("RunTimerCanvas");
        canvasGO.transform.SetParent(transform, false);
        DontDestroyOnLoad(canvasGO);

        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 10;

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Label centré en bas de l'écran
        GameObject labelGO = new GameObject("TimerLabel");
        labelGO.transform.SetParent(canvasGO.transform, false);

        _label = labelGO.AddComponent<TextMeshProUGUI>();
        _label.alignment = TextAlignmentOptions.Center;
        _label.fontSize  = _fontSize;
        _label.color     = _textColor;
        _label.fontStyle = FontStyles.Bold;

        TMP_FontAsset tiny5 = LoadFont();
        if (tiny5 != null)
            _label.font = tiny5;
        else
            Debug.LogWarning($"[RunTimerUI] Font Tiny5 introuvable à : {FontAssetPath}");

        // Ancré en bas au centre
        RectTransform rect = _label.rectTransform;
        rect.anchorMin        = new Vector2(0.5f, 0f);
        rect.anchorMax        = new Vector2(0.5f, 0f);
        rect.pivot            = new Vector2(0.5f, 0f);
        rect.sizeDelta        = new Vector2(300f, 50f);
        rect.anchoredPosition = new Vector2(0f, 20f);
    }

    private void UpdateLabel(float elapsed)
    {
        if (_label != null)
            _label.text = RunTimerManager.FormatTime(elapsed);
    }

    // Charge la font Tiny5 depuis l'éditeur ou via Resources en build
    private TMP_FontAsset LoadFont()
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
#else
        return Resources.Load<TMP_FontAsset>("Tiny5-Regular SDF");
#endif
    }
}
