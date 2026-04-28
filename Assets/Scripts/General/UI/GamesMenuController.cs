using UnityEngine;
using UnityEngine.SceneManagement;

// Gère le panneau "All Games" du menu principal.
// Ouvre/ferme le panneau et lance chaque mini-jeu en mode isolé (IsMinigameMode).
public class GamesMenuController : MonoBehaviour
{
    // Panneau All Games à afficher/cacher
    [SerializeField] private GameObject _gamesPanel;

    // Panneau menu principal à cacher pendant l'affichage du panneau jeux
    [SerializeField] private GameObject _menuPanel;

    // Son joué lors des interactions boutons
    [SerializeField] private AudioClip _buttonClip;

    // -------------------------------------------------------------------------
    // Noms des scènes mini-jeux (étages simples)
    // -------------------------------------------------------------------------

    // Bullet Hell : étage 1 seul
    private const string SceneBulletHell    = "Scene_BulletHell";

    // Game & Watch : scène dédiée
    private const string SceneGameAndWatch  = "Scene_GameAndWatch";

    // Space Invaders : scène dédiée
    private const string SceneSpaceInvaders = "Scene_SpaceInvaders";

    // -------------------------------------------------------------------------
    // Navigation
    // -------------------------------------------------------------------------

    /// <summary>Ouvre le panneau All Games depuis le menu principal.</summary>
    public void OnAllGamesClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        _menuPanel.SetActive(false);
        _gamesPanel.SetActive(true);
    }

    /// <summary>Ferme le panneau All Games et retourne au menu principal.</summary>
    public void OnBackClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        _gamesPanel.SetActive(false);
        _menuPanel.SetActive(true);
    }

    // -------------------------------------------------------------------------
    // Lancement des mini-jeux
    // -------------------------------------------------------------------------

    /// <summary>Lance Bullet Hell en mode mini-jeu isolé (étage 1).</summary>
    public void OnBulletHellClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        LaunchMinigame(SceneBulletHell, floor: 1);
    }

    /// <summary>Lance Game & Watch en mode mini-jeu isolé (étage 4).</summary>
    public void OnGameAndWatchClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        LaunchMinigame(SceneGameAndWatch, floor: 4);
    }

    /// <summary>Lance Space Invaders en mode mini-jeu isolé (étage 5).</summary>
    public void OnSpaceInvadersClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        LaunchMinigame(SceneSpaceInvaders, floor: 5);
    }

    // -------------------------------------------------------------------------
    // Interne
    // -------------------------------------------------------------------------

    // Réinitialise GameProgress en mode mini-jeu puis charge la scène
    private void LaunchMinigame(string sceneName, int floor)
    {
        if (GameProgress.Instance == null)
        {
            Debug.LogError("[GamesMenuController] GameProgress.Instance est null.", this);
            return;
        }

        GameProgress.Instance.Reset();
        GameProgress.Instance.SetMinigameMode(floor);
        SaveSystem.DeleteSave();

        SceneManager.LoadScene(sceneName);
    }
}
