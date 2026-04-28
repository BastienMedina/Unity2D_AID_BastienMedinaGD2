using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RunTimerUI : MonoBehaviour
{
    [SerializeField] private float _fontSize  = 40f;
    [SerializeField] private Color _textColor = Color.white;

    private const string FontAssetPath = "Assets/TextMesh Pro/Fonts/Tiny5-Regular SDF.asset";

    private TextMeshProUGUI _label;
    private Canvas          _canvas;

    private void Awake() // Persiste l'objet et construit le Canvas
    {
        DontDestroyOnLoad(gameObject);
        BuildCanvas();
    }

    private void OnEnable() // Abonne le callback de tick à RunTimerManager
    {
        if (RunTimerManager.Instance != null)
            RunTimerManager.Instance.OnTick.AddListener(OnTick);
    }

    private void OnDisable() // Désabonne le callback de tick
    {
        if (RunTimerManager.Instance != null)
            RunTimerManager.Instance.OnTick.RemoveListener(OnTick);
    }

    private void Start() // Abonne OnRunFinished et masque le timer
    {
        if (RunTimerManager.Instance != null)
        {
            RunTimerManager.Instance.OnTick.AddListener(OnTick);
            RunTimerManager.Instance.OnRunFinished.AddListener(OnRunFinished);
        }
        UpdateLabel(0f);
        SetVisible(false);
    }

    public void SetVisible(bool visible) { if (_canvas != null) _canvas.gameObject.SetActive(visible); } // Affiche ou cache le timer

    private void OnTick(float elapsed) // Révèle et met à jour chaque seconde
    {
        SetVisible(true);
        UpdateLabel(elapsed);
    }

    private void OnRunFinished(float elapsed, bool _) => UpdateLabel(elapsed); // Met à jour le label à la fin de run

    private void BuildCanvas() // Crée un Canvas overlay persistant avec label centré en bas
    {
        GameObject canvasGO = new GameObject("RunTimerCanvas");
        canvasGO.transform.SetParent(transform, false);
        DontDestroyOnLoad(canvasGO);

        _canvas              = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 10;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject labelGO = new GameObject("TimerLabel");
        labelGO.transform.SetParent(canvasGO.transform, false);

        _label           = labelGO.AddComponent<TextMeshProUGUI>();
        _label.alignment = TextAlignmentOptions.Center;
        _label.fontSize  = _fontSize;
        _label.color     = _textColor;
        _label.fontStyle = FontStyles.Bold;

        TMP_FontAsset tiny5 = LoadFont();
        if (tiny5 != null) _label.font = tiny5;

        RectTransform rect    = _label.rectTransform;
        rect.anchorMin        = new Vector2(0.5f, 0f);
        rect.anchorMax        = new Vector2(0.5f, 0f);
        rect.pivot            = new Vector2(0.5f, 0f);
        rect.sizeDelta        = new Vector2(300f, 50f);
        rect.anchoredPosition = new Vector2(0f, 20f);
    }

    private void UpdateLabel(float elapsed) // Met à jour le texte du timer
    {
        if (_label != null) _label.text = RunTimerManager.FormatTime(elapsed);
    }

    private TMP_FontAsset LoadFont() // Charge la font selon l'éditeur ou un build
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
#else
        return Resources.Load<TMP_FontAsset>("Tiny5-Regular SDF");
#endif
    }
}
