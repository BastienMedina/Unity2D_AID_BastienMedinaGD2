using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverMenuController : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _floorLabel;
    [SerializeField] private string _restartScene  = "Scene_BulletHell";
    [SerializeField] private string _mainMenuScene = "Scene_MainMenu";
    [SerializeField] private AudioClip _buttonClip;

    private void Awake() // Cache le panneau au démarrage
    {
        if (_panel != null) _panel.SetActive(false);
    }

    private void Start() // Abonne HandleDeath et câble les boutons
    {
        if (LivesManager.Instance == null) return;
        LivesManager.Instance.OnDeath.AddListener(HandleDeath);

        Button btnRestart  = FindButtonInChildren(_panel.transform, "Button_Restart");
        Button btnMainMenu = FindButtonInChildren(_panel.transform, "Button_MainMenu");
        btnRestart?.onClick.AddListener(OnRestartClicked);
        btnMainMenu?.onClick.AddListener(OnMainMenuClicked);
    }

    private void OnDestroy() // Désabonne le listener de mort
    {
        LivesManager.Instance?.OnDeath.RemoveListener(HandleDeath);
    }

    public void HandleDeath() // Gère le mode mini-jeu puis affiche le panneau
    {
        if (GameProgress.Instance != null && GameProgress.Instance.IsMinigameMode) // Mini-jeu : retour menu
        {
            MinigameReturnHandler minigame = FindFirstObjectByType<MinigameReturnHandler>();
            if (minigame != null) { minigame.ReturnToMenu(); return; }
        }

        if (_floorLabel != null && GameProgress.Instance != null)
            _floorLabel.text = "ÉTAGE " + GameProgress.Instance.CurrentFloor;

        PauseManager.Instance?.LockForEndState();
        _panel.SetActive(true);
    }

    public void OnRestartClicked() // Réinitialise et charge la scène de départ
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        GameProgress.Instance?.Reset();
        SaveSystem.DeleteSave();
        Time.timeScale = 1f;
        SceneManager.LoadScene(_restartScene);
    }

    public void OnMainMenuClicked() // Réinitialise et charge le menu principal
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        GameProgress.Instance?.Reset();
        SaveSystem.DeleteSave();
        Time.timeScale = 1f;
        SceneManager.LoadScene(_mainMenuScene);
    }

    private Button FindButtonInChildren(Transform root, string buttonName) // Cherche récursivement un bouton par nom
    {
        if (root == null) return null;
        Transform found = root.Find(buttonName);
        if (found != null) return found.GetComponent<Button>();
        foreach (Transform child in root) { Button result = FindButtonInChildren(child, buttonName); if (result != null) return result; }
        return null;
    }
}
