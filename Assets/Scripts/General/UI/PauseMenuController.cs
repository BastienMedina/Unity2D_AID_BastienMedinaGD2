using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private string _mainMenuScene = "Scene_MainMenu";
    [SerializeField] private AudioClip _buttonClip;

    private void Awake() // Cache le panneau au démarrage
    {
        if (_panel != null) _panel.SetActive(false);
    }

    private void Start() // Câble les boutons Reprendre et Menu principal
    {
        if (PauseManager.Instance == null) return;
        Button btnResume   = FindButtonInChildren(_panel.transform, "Button_Resume");
        Button btnMainMenu = FindButtonInChildren(_panel.transform, "Button_MainMenu");
        btnResume?.onClick.AddListener(OnResumeClicked);
        btnMainMenu?.onClick.AddListener(OnMainMenuClicked);
    }

    public void Show() { if (_panel != null) _panel.SetActive(true);  } // Affiche le panneau pause

    public void Hide() { if (_panel != null) _panel.SetActive(false); } // Masque le panneau pause

    public void OnResumeClicked() // Cache le panneau et reprend via PauseManager
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        Hide();
        PauseManager.Instance?.Resume();
    }

    public void OnMainMenuClicked() // Sauvegarde, synchronise le PauseManager et charge le menu
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        SaveSystem.SaveGame();
        PauseManager.Instance?.Resume(); // Synchronise l'état interne avant de charger
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
