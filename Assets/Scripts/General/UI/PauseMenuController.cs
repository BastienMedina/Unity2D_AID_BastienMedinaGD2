using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Contrôle le menu pause : s'abonne au PauseManager, affiche/masque le panneau,
// et garantit que le Canvas ne bloque jamais les raycast quand il est inactif.
public class PauseMenuController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Références sérialisées — à assigner dans l'Inspector
    // -------------------------------------------------------------------------

    // GraphicRaycaster du Canvas racine — désactivé quand le menu est fermé
    [SerializeField] private GraphicRaycaster _raycaster;

    // Panneau de contenu visible (titre, boutons)
    [SerializeField] private GameObject _panel;

    // Nom de la scène du menu principal
    [SerializeField] private string _mainMenuScene = "Scene_MainMenu";

    // Son joué lors des interactions avec les boutons
    [SerializeField] private AudioClip _buttonClip;

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    // Référence au PauseManager de la scène, résolu dans Awake
    private PauseManager _pauseManager;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    private void Awake()
    {
        _pauseManager = FindFirstObjectByType<PauseManager>();

        Debug.Log($"[PauseMenuController] Awake — _pauseManager : {(_pauseManager != null ? _pauseManager.name : "NULL")}", this);
        Debug.Log($"[PauseMenuController] Awake — _raycaster : {(_raycaster != null ? _raycaster.name : "NULL")}", this);
        Debug.Log($"[PauseMenuController] Awake — _panel : {(_panel != null ? _panel.name : "NULL")}", this);

        if (_pauseManager == null)
            Debug.LogWarning("[PauseMenuController] Aucun PauseManager trouvé dans la scène.", this);

        if (_raycaster == null)
            Debug.LogError("[PauseMenuController] _raycaster non assigné.", this);

        if (_panel == null)
            Debug.LogError("[PauseMenuController] _panel non assigné.", this);

        SetMenuVisible(false);
    }

    private void Start()
    {
        Button btnResume   = FindButtonInChildren(_panel.transform, "Button_Resume");
        Button btnMainMenu = FindButtonInChildren(_panel.transform, "Button_MainMenu");

        Debug.Log($"[PauseMenuController] Start — Button_Resume : {(btnResume != null ? btnResume.name : "NULL")}", this);
        Debug.Log($"[PauseMenuController] Start — Button_MainMenu : {(btnMainMenu != null ? btnMainMenu.name : "NULL")}", this);

        if (btnResume != null)
            btnResume.onClick.AddListener(OnResumeClicked);
        else
            Debug.LogWarning("[PauseMenuController] Button_Resume introuvable sous _panel.", this);

        if (btnMainMenu != null)
            btnMainMenu.onClick.AddListener(OnMainMenuClicked);
        else
            Debug.LogWarning("[PauseMenuController] Button_MainMenu introuvable sous _panel.", this);
    }

    private void OnEnable()
    {
        if (_pauseManager == null) return;
        Debug.Log("[PauseMenuController] OnEnable — abonnement à _onPaused et _onResumed.", this);
        _pauseManager._onPaused.AddListener(Show);
        _pauseManager._onResumed.AddListener(Hide);
    }

    private void OnDisable()
    {
        if (_pauseManager == null) return;
        Debug.Log("[PauseMenuController] OnDisable — désabonnement.", this);
        _pauseManager._onPaused.RemoveListener(Show);
        _pauseManager._onResumed.RemoveListener(Hide);
    }

    // -------------------------------------------------------------------------
    // Boutons
    // -------------------------------------------------------------------------

    /// <summary>Reprend le jeu — le PauseManager déclenche Hide via _onResumed.</summary>
    public void OnResumeClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        _pauseManager?.Resume();
    }

    /// <summary>Sauvegarde, reprend le timeScale via le PauseManager et charge le menu principal.</summary>
    public void OnMainMenuClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        SaveSystem.SaveGame();

        // Passe par Resume() pour synchroniser _isPaused et notifier tous les abonnés
        _pauseManager?.Resume();

        SceneManager.LoadScene(_mainMenuScene);
    }

    // -------------------------------------------------------------------------
    // Visibilité
    // -------------------------------------------------------------------------

    // Affiche le menu et active le raycaster pour recevoir les interactions
    private void Show()
    {
        Debug.Log($"[PauseMenuController] Show() appelé — _panel : {(_panel != null ? _panel.name : "NULL")}", this);
        SetMenuVisible(true);
        Debug.Log($"[PauseMenuController] Show() terminé — _panel.activeSelf : {_panel?.activeSelf}, _raycaster.enabled : {_raycaster?.enabled}", this);
    }

    // Masque le menu et désactive le raycaster pour ne jamais bloquer l'UI sous-jacente
    private void Hide()
    {
        Debug.Log("[PauseMenuController] Hide() appelé.", this);
        SetMenuVisible(false);
    }

    // Cherche récursivement un bouton par nom dans toute la hiérarchie enfant.
    private Button FindButtonInChildren(Transform root, string buttonName)
    {
        Transform found = root.Find(buttonName);
        if (found != null)
            return found.GetComponent<Button>();

        foreach (Transform child in root)
        {
            Button result = FindButtonInChildren(child, buttonName);
            if (result != null)
                return result;
        }

        return null;
    }

    // Centralise l'activation du panneau ET du raycaster en un seul appel atomique
    private void SetMenuVisible(bool visible)
    {
        if (_panel != null)
            _panel.SetActive(visible);

        // Désactiver le GraphicRaycaster = zéro interception de clics sur ce Canvas,
        // peu importe l'état des Image enfants ou les overrides de scène.
        if (_raycaster != null)
            _raycaster.enabled = visible;
    }
}
