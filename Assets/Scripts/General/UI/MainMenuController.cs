using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private GameObject _optionsPanel;
    [SerializeField] private Button _continueButton;
    [SerializeField] private string _bulletHellScene = "Scene_BulletHell";
    [SerializeField] private AudioClip _buttonClip;

    private void Start() // Initialise l'état des panneaux et du bouton continuer
    {
        _menuPanel.SetActive(true);
        _optionsPanel.SetActive(false);
        _continueButton.interactable = SaveSystem.HasSave(); // Active si une save existe
    }

    public void OnPlayClicked() // Réinitialise, démarre le timer et charge BulletHell
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        if (GameProgress.Instance == null) return;
        GameProgress.Instance.Reset();
        SaveSystem.DeleteSave();
        RunTimerManager.Instance?.StartRun();
        SceneManager.LoadScene(_bulletHellScene);
    }

    public void OnContinueClicked() // Charge la save et navigue vers la scène correspondante
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        if (!SaveSystem.HasSave() || GameProgress.Instance == null) return;
        SaveSystem.LoadGame();
        RunTimerManager.Instance?.StartRun();
        SceneManager.LoadScene(GameProgress.Instance.GetCurrentSceneName());
    }

    public void OnOptionsClicked() // Bascule l'affichage vers le panneau des options
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        _menuPanel.SetActive(false);
        _optionsPanel.SetActive(true);
    }
}
