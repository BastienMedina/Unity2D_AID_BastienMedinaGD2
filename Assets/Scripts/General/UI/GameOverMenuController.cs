using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Affiche l'écran de défaite quand LivesManager.OnDeath est déclenché.
/// Gèle le jeu via PauseManager.LockForEndState() pour bloquer toute pause ultérieure.
/// À placer sur le Canvas game over dans chaque scène de jeu.
/// </summary>
public class GameOverMenuController : MonoBehaviour
{
    // Panneau racine du menu game over
    [SerializeField] private GameObject _panel;

    // Texte affichant l'étage atteint par le joueur (optionnel)
    [SerializeField] private TextMeshProUGUI _floorLabel;

    // Nom de la scène de départ (premier étage)
    [SerializeField] private string _restartScene = "Scene_BulletHell";

    // Nom de la scène du menu principal
    [SerializeField] private string _mainMenuScene = "Scene_MainMenu";

    // Son joué lors des interactions avec les boutons
    [SerializeField] private AudioClip _buttonClip;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (_panel != null)
            _panel.SetActive(false);
    }

    private void Start()
    {
        if (LivesManager.Instance == null)
        {
            Debug.LogError("[GameOverMenuController] LivesManager.Instance introuvable.", this);
            return;
        }

        LivesManager.Instance.OnDeath.AddListener(HandleDeath);

        Button btnRestart  = FindButtonInChildren(_panel.transform, "Button_Restart");
        Button btnMainMenu = FindButtonInChildren(_panel.transform, "Button_MainMenu");

        btnRestart?.onClick.AddListener(OnRestartClicked);
        btnMainMenu?.onClick.AddListener(OnMainMenuClicked);
    }

    private void OnDestroy()
    {
        LivesManager.Instance?.OnDeath.RemoveListener(HandleDeath);
    }

    // -------------------------------------------------------------------------
    // Déclenchement défaite
    // -------------------------------------------------------------------------

    /// <summary>Appelé par LivesManager.OnDeath — affiche le panneau et gèle le jeu.</summary>
    public void HandleDeath()
    {
        // En mode mini-jeu : retourne directement au menu sans afficher l'écran game over
        if (GameProgress.Instance != null && GameProgress.Instance.IsMinigameMode)
        {
            MinigameReturnHandler minigame = FindFirstObjectByType<MinigameReturnHandler>();
            if (minigame != null)
            {
                minigame.ReturnToMenu();
                return;
            }
        }

        if (_floorLabel != null && GameProgress.Instance != null)
            _floorLabel.text = "ÉTAGE " + GameProgress.Instance.CurrentFloor;

        PauseManager.Instance?.LockForEndState();
        _panel.SetActive(true);
    }

    // -------------------------------------------------------------------------
    // Boutons
    // -------------------------------------------------------------------------

    /// <summary>Repart depuis le début sans sauvegarde.</summary>
    public void OnRestartClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        GameProgress.Instance?.Reset();
        SaveSystem.DeleteSave();
        Time.timeScale = 1f;
        SceneManager.LoadScene(_restartScene);
    }

    /// <summary>Retourne au menu principal sans sauvegarder.</summary>
    public void OnMainMenuClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        GameProgress.Instance?.Reset();
        SaveSystem.DeleteSave();
        Time.timeScale = 1f;
        SceneManager.LoadScene(_mainMenuScene);
    }

    // -------------------------------------------------------------------------
    // Utilitaire
    // -------------------------------------------------------------------------

    private Button FindButtonInChildren(Transform root, string buttonName)
    {
        if (root == null)
            return null;

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
}
