using UnityEngine;
using UnityEngine.UI;

// Bouton flottant toujours visible qui bascule le menu pause.
public class QuickMenuController : MonoBehaviour
{
    // Bouton flottant (le petit bouton pause en coin d'écran)
    [SerializeField] private Button _quickButton;

    // Son joué lors du clic
    [SerializeField] private AudioClip _buttonClip;

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    private PauseManager _pauseManager;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    private void Awake()
    {
        _pauseManager = FindFirstObjectByType<PauseManager>();

        Debug.Log($"[QuickMenuController] Awake — _pauseManager : {(_pauseManager != null ? _pauseManager.name : "NULL")}", this);
        Debug.Log($"[QuickMenuController] Awake — _quickButton : {(_quickButton != null ? _quickButton.name : "NULL")}", this);

        if (_pauseManager == null)
            Debug.LogError("[QuickMenuController] Aucun PauseManager trouvé dans la scène.", this);

        if (_quickButton == null)
            Debug.LogError("[QuickMenuController] _quickButton non assigné.", this);
    }

    private void Start()
    {
        if (_quickButton != null)
        {
            _quickButton.onClick.AddListener(OnQuickButtonClicked);
            Debug.Log($"[QuickMenuController] Start — listener AddListener OK. Bouton interactable : {_quickButton.interactable}", this);
        }
    }

    private void OnDestroy()
    {
        if (_quickButton != null)
            _quickButton.onClick.RemoveListener(OnQuickButtonClicked);
    }

    // -------------------------------------------------------------------------
    // Bouton
    // -------------------------------------------------------------------------

    /// <summary>Bascule le menu pause via le PauseManager.</summary>
    public void OnQuickButtonClicked()
    {
        Debug.Log($"[QuickMenuController] OnQuickButtonClicked — _pauseManager : {(_pauseManager != null ? _pauseManager.name : "NULL")}", this);
        AudioManager.Instance?.PlaySFX(_buttonClip);
        _pauseManager?.TogglePause();
    }
}
