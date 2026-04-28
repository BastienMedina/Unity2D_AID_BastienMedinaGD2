using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryMenuController : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private string _mainMenuScene = "Scene_MainMenu";
    [SerializeField] private AudioClip _buttonClip;

    private void Awake() // Cache le panneau au démarrage
    {
        if (_panel != null) _panel.SetActive(false);
    }

    private void Start() // Câble HandleVictory et le bouton menu
    {
        SI_ProgressionGauge gauge = FindFirstObjectByType<SI_ProgressionGauge>();
        if (gauge != null) gauge.OnVictory.AddListener(HandleVictory);

        Button btnMainMenu = FindButtonInChildren(_panel.transform, "Button_MainMenu");
        btnMainMenu?.onClick.AddListener(OnMainMenuClicked);
    }

    public void HandleVictory() // Arrête le timer, gèle le jeu et affiche l'écran victoire
    {
        MinigameReturnHandler minigame = FindFirstObjectByType<MinigameReturnHandler>();
        if (minigame != null) { minigame.ReturnToMenu(); return; } // Mini-jeu : ignore l'écran victoire

        RunTimerManager.Instance?.StopRun(); // Enregistre le meilleur temps
        PauseManager.Instance?.LockForEndState();
        _panel.SetActive(true);
    }

    public void OnMainMenuClicked() // Réinitialise et charge le menu principal
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        SaveSystem.DeleteSave();
        GameProgress.Instance?.Reset();
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
