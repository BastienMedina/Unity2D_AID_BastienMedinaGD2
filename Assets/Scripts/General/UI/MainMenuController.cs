using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Contrôle la navigation du menu principal
public class MainMenuController : MonoBehaviour
{
    // Panneau racine du menu principal
    [SerializeField] private GameObject _menuPanel;

    // Panneau des options à afficher
    [SerializeField] private GameObject _optionsPanel;

    // Bouton continuer activé selon la save
    [SerializeField] private Button _continueButton;

    // Nom de la scène du mode principal
    [SerializeField] private string _bulletHellScene = "Scene_BulletHell";

    // Son joué lors des interactions avec les boutons du menu
    [SerializeField] private AudioClip _buttonClip;

    // Initialise l'état des panneaux et boutons au démarrage
    private void Start()
    {
        // Affiche le panneau menu au lancement
        _menuPanel.SetActive(true);

        // Cache les options au démarrage du menu
        _optionsPanel.SetActive(false);

        // Active le bouton continue si une save existe
        _continueButton.interactable = SaveSystem.HasSave();
    }

    /// <summary>Lance une nouvelle partie depuis le début au floor 1.</summary>
    // Réinitialise la progression, efface la save existante et charge le BulletHell
    public void OnPlayClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);

        // Protège contre l'absence du singleton GameProgress dans la scène
        if (GameProgress.Instance == null)
        {
            Debug.LogError("[MainMenuController] GameProgress.Instance est null — vérifie que GO_GameProgress est présent dans Scene_MainMenu.");
            return;
        }

        // Repart toujours à l'étage 1 et efface la save précédente
        GameProgress.Instance.Reset();
        SaveSystem.DeleteSave();

        SceneManager.LoadScene(_bulletHellScene);
    }

    /// <summary>Reprend la partie à la scène et l'étage sauvegardés.</summary>
    // Charge la save et navigue vers la scène correspondante
    public void OnContinueClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);

        // Ignore si aucune sauvegarde n'est disponible
        if (!SaveSystem.HasSave()) return;

        // Protège contre l'absence du singleton GameProgress dans la scène
        if (GameProgress.Instance == null)
        {
            Debug.LogError("[MainMenuController] GameProgress.Instance est null — vérifie que GO_GameProgress est présent dans Scene_MainMenu.");
            return;
        }

        SaveSystem.LoadGame();

        // Charge la scène correspondant à l'étage sauvegardé
        SceneManager.LoadScene(GameProgress.Instance.GetCurrentSceneName());
    }

    /// <summary>Affiche le panneau des options et cache le menu.</summary>
    // Bascule l'affichage vers le panneau des options
    public void OnOptionsClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        _menuPanel.SetActive(false);
        _optionsPanel.SetActive(true);
    }
}
