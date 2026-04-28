using UnityEngine;
using UnityEngine.SceneManagement;

public class GamesMenuController : MonoBehaviour
{
    [SerializeField] private GameObject _gamesPanel;
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private AudioClip _buttonClip;

    private const string SceneBulletHell    = "Scene_BulletHell";
    private const string SceneGameAndWatch  = "Scene_GameAndWatch";
    private const string SceneSpaceInvaders = "Scene_SpaceInvaders";

    public void OnAllGamesClicked() // Cache le menu et affiche le panneau jeux
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        _menuPanel.SetActive(false);
        _gamesPanel.SetActive(true);
    }

    public void OnBackClicked() // Cache le panneau jeux et affiche le menu
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        _gamesPanel.SetActive(false);
        _menuPanel.SetActive(true);
    }

    public void OnBulletHellClicked()    { AudioManager.Instance?.PlaySFX(_buttonClip); LaunchMinigame(SceneBulletHell,    floor: 1); } // Lance Bullet Hell en mini-jeu
    public void OnGameAndWatchClicked()  { AudioManager.Instance?.PlaySFX(_buttonClip); LaunchMinigame(SceneGameAndWatch,  floor: 4); } // Lance Game & Watch en mini-jeu
    public void OnSpaceInvadersClicked() { AudioManager.Instance?.PlaySFX(_buttonClip); LaunchMinigame(SceneSpaceInvaders, floor: 5); } // Lance Space Invaders en mini-jeu

    private void LaunchMinigame(string sceneName, int floor) // Réinitialise, active mini-jeu et charge la scène
    {
        if (GameProgress.Instance == null) return;
        GameProgress.Instance.Reset();
        GameProgress.Instance.SetMinigameMode(floor);
        SaveSystem.DeleteSave();
        SceneManager.LoadScene(sceneName);
    }
}
