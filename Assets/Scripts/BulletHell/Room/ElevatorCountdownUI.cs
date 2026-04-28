using TMPro;
using UnityEngine;

[RequireComponent(typeof(ElevatorDoorController))]
public class ElevatorCountdownUI : MonoBehaviour
{
    [SerializeField] private Vector3 _localOffset = new Vector3(0f, 0.15f, 0f);
    [SerializeField] private float _fontSize = 7f;
    [SerializeField] private Color _colorNormal = Color.white;
    [SerializeField] private Color _colorUrgent = new Color(1f, 0.25f, 0.1f, 1f);
    [SerializeField] private int _urgentThreshold = 10;
    [SerializeField] private int _sortingOrder = 5;

    private const string FontAssetPath = "Assets/TextMesh Pro/Fonts/Tiny5-Regular SDF.asset";
    private ElevatorDoorController _doorController;
    private TextMeshPro _label;

    private void Awake() // Récupère le controller et construit le label
    {
        _doorController = GetComponent<ElevatorDoorController>();
        BuildLabel();
    }

    private void Start() // Affiche le label dès le démarrage
    {
        _label.gameObject.SetActive(true);
    }

    private void OnEnable() // S'abonne aux événements tick et ouverture
    {
        _doorController.OnTimerTick.AddListener(OnTick);
        _doorController.OnDoorsOpened.AddListener(HideLabel);
    }

    private void OnDisable() // Désabonne les listeners
    {
        _doorController.OnTimerTick.RemoveListener(OnTick);
        _doorController.OnDoorsOpened.RemoveListener(HideLabel);
    }

    private void HideLabel() // Cache le label quand les portes s'ouvrent
    {
        _label.gameObject.SetActive(false);
    }

    private void OnTick(int secondsRemaining) // Met à jour texte et couleur du timer
    {
        int minutes    = secondsRemaining / 60;
        int seconds    = secondsRemaining % 60;
        _label.text    = $"{minutes:D2}:{seconds:D2}";
        _label.color   = secondsRemaining <= _urgentThreshold ? _colorUrgent : _colorNormal;
    }

    private void BuildLabel() // Crée le TextMeshPro enfant en world space
    {
        GameObject labelGO = new GameObject("Countdown_Label");
        labelGO.transform.SetParent(transform, false);
        labelGO.transform.localPosition = _localOffset;

        Vector3 parentScale     = transform.lossyScale;
        float scaleCompensation = 1f / Mathf.Max(parentScale.x, 0.001f); // Compense l'échelle parent
        labelGO.transform.localScale = Vector3.one * scaleCompensation;

        _label              = labelGO.AddComponent<TextMeshPro>();
        _label.alignment    = TextAlignmentOptions.Center;
        _label.fontSize     = _fontSize;
        _label.color        = _colorNormal;
        _label.fontStyle    = FontStyles.Bold;
        _label.sortingOrder = _sortingOrder;

        TMP_FontAsset tiny5 = LoadFont();
        if (tiny5 != null)
            _label.font = tiny5;
        else
            Debug.LogWarning($"[ElevatorCountdownUI] Font Tiny5 introuvable à : {FontAssetPath}");

        RectTransform rect    = _label.rectTransform;
        rect.sizeDelta        = new Vector2(2f, 0.6f);
        rect.anchoredPosition = Vector2.zero;
    }

    private TMP_FontAsset LoadFont() // Charge la font selon le contexte éditeur ou build
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
#else
        return Resources.Load<TMP_FontAsset>("Tiny5-Regular SDF");
#endif
    }
}
