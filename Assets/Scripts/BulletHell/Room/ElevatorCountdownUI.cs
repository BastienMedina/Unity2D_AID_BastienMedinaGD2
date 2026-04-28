using TMPro;
using UnityEngine;

// Construit et pilote un texte de compte à rebours en world space dans l'ascenseur.
// S'abonne à ElevatorDoorController.OnTimerTick pour recevoir le temps restant.
// Crée son propre GameObject TextMeshPro enfant — aucun prefab requis.
[RequireComponent(typeof(ElevatorDoorController))]
public class ElevatorCountdownUI : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Paramètres configurables
    // -------------------------------------------------------------------------

    // Position locale du texte dans l'ascenseur (en unités monde)
    [SerializeField] private Vector3 _localOffset = new Vector3(0f, 0.15f, 0f);

    // Taille de la police en unités monde
    [SerializeField] private float _fontSize = 7f;

    // Couleur du texte quand le timer est haut
    [SerializeField] private Color _colorNormal = Color.white;

    // Couleur du texte quand le timer est bas (seuil configurable)
    [SerializeField] private Color _colorUrgent = new Color(1f, 0.25f, 0.1f, 1f);

    // Seuil en secondes sous lequel la couleur urgente s'active
    [SerializeField] private int _urgentThreshold = 10;

    // Ordre de tri du texte (doit être au-dessus des portes)
    [SerializeField] private int _sortingOrder = 5;

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    private ElevatorDoorController _doorController;
    private TextMeshPro _label;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    private void Awake()
    {
        _doorController = GetComponent<ElevatorDoorController>();
        BuildLabel();
    }

    // Affiche le label immédiatement dès le démarrage de la scène
    private void Start()
    {
        _label.gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        _doorController.OnTimerTick.AddListener(OnTick);
        _doorController.OnDoorsOpened.AddListener(HideLabel);
    }

    private void OnDisable()
    {
        _doorController.OnTimerTick.RemoveListener(OnTick);
        _doorController.OnDoorsOpened.RemoveListener(HideLabel);
    }

    // -------------------------------------------------------------------------
    // Callbacks
    // -------------------------------------------------------------------------

    /// <summary>Cache le label quand les portes s'ouvrent (fin du timer).</summary>
    private void HideLabel()
    {
        _label.gameObject.SetActive(false);
    }

    /// <summary>Met à jour le texte et la couleur à chaque tick du timer.</summary>
    private void OnTick(int secondsRemaining)
    {
        int minutes = secondsRemaining / 60;
        int seconds = secondsRemaining % 60;

        _label.text = $"{minutes:D2}:{seconds:D2}";
        _label.color = secondsRemaining <= _urgentThreshold ? _colorUrgent : _colorNormal;
    }

    // -------------------------------------------------------------------------
    // Construction du label
    // -------------------------------------------------------------------------

    // Chemin de la font TMP SDF utilisée pour le label
    private const string FontAssetPath = "Assets/TextMesh Pro/Fonts/Tiny5-Regular SDF.asset";

    // Instancie un TextMeshPro en world space comme enfant de l'ascenseur
    private void BuildLabel()
    {
        GameObject labelGO = new GameObject("Countdown_Label");
        labelGO.transform.SetParent(transform, false);
        labelGO.transform.localPosition = _localOffset;

        // Compense l'échelle du parent pour que la taille de police reste constante
        Vector3 parentScale = transform.lossyScale;
        float scaleCompensation = 1f / Mathf.Max(parentScale.x, 0.001f);
        labelGO.transform.localScale = Vector3.one * scaleCompensation;

        _label = labelGO.AddComponent<TextMeshPro>();
        _label.alignment = TextAlignmentOptions.Center;
        _label.fontSize = _fontSize;
        _label.color = _colorNormal;
        _label.fontStyle = FontStyles.Bold;
        _label.sortingOrder = _sortingOrder;

        // Charge et applique la font Tiny5
        TMP_FontAsset tiny5 = LoadFont();
        if (tiny5 != null)
            _label.font = tiny5;
        else
            Debug.LogWarning($"[ElevatorCountdownUI] Font Tiny5 introuvable à : {FontAssetPath}");

        // Redimensionne la RectTransform pour centrer correctement le texte
        RectTransform rect = _label.rectTransform;
        rect.sizeDelta = new Vector2(2f, 0.6f);
        rect.anchoredPosition = Vector2.zero;
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
