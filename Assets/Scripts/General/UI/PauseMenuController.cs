using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Contrôle le panneau du menu pause — câble les boutons et s'abonne au PauseManager.
public class PauseMenuController : MonoBehaviour
{
    // Panneau racine du menu pause
    [SerializeField] private GameObject _pausePanel;

    // Nom de la scène du menu principal
    [SerializeField] private string _mainMenuScene = "Scene_MainMenu";

    // Son joué lors des interactions avec les boutons
    [SerializeField] private AudioClip _buttonClip;

    // Référence au PauseManager local de la scène
    private PauseManager _pauseManager;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    private void Awake()
    {
        _pauseManager = FindFirstObjectByType<PauseManager>();

        if (_pauseManager == null)
            Debug.LogWarning("[PauseMenuController] Aucun PauseManager trouvé dans la scène.");

        // Cache le panneau au démarrage
        _pausePanel.SetActive(false);
    }

    private void Start()
    {
        // Câblage des boutons par code — évite la dépendance aux persistentCalls dans l'Inspector
        Button btnResume   = _pausePanel.transform.Find("Panel/Button_Resume")?.GetComponent<Button>();
        Button btnMainMenu = _pausePanel.transform.Find("Panel/Button_MainMenu")?.GetComponent<Button>();

        btnResume?.onClick.AddListener(OnResumeClicked);
        btnMainMenu?.onClick.AddListener(OnMainMenuClicked);
    }

    private void OnEnable()
    {
        if (_pauseManager == null) return;

        _pauseManager._onPaused.AddListener(ShowPanel);
        _pauseManager._onResumed.AddListener(HidePanel);
    }

    private void OnDisable()
    {
        if (_pauseManager == null) return;

        _pauseManager._onPaused.RemoveListener(ShowPanel);
        _pauseManager._onResumed.RemoveListener(HidePanel);
    }

    // -------------------------------------------------------------------------
    // API publique — boutons
    // -------------------------------------------------------------------------

    /// <summary>Reprend la partie et cache le panneau.</summary>
    public void OnResumeClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        _pauseManager?.Resume();
    }

    /// <summary>Sauvegarde la progression, réinitialise le timeScale et retourne au menu.</summary>
    public void OnMainMenuClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);

        // Sauvegarde l'étage et les vies avant de quitter
        SaveSystem.SaveGame();

        // Restaure le timeScale avant de changer de scène
        Time.timeScale = 1f;

        SceneManager.LoadScene(_mainMenuScene);
    }

    // -------------------------------------------------------------------------
    // Affichage
    // -------------------------------------------------------------------------

    private void ShowPanel() => _pausePanel.SetActive(true);
    private void HidePanel() => _pausePanel.SetActive(false);
}
