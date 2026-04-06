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

    /// <summary>Lance ou reprend une partie depuis le bouton PLAY.</summary>
    // Charge la save si elle existe, sinon repart à zéro
    public void OnPlayClicked()
    {
        if (SaveSystem.HasSave())
        {
            // Restaure la progression existante avant de charger
            SaveSystem.LoadGame();
        }
        else
        {
            // Remet le singleton à l'étage 1 pour une nouvelle partie
            GameProgress.Instance.Reset();
        }

        SceneManager.LoadScene(_bulletHellScene);
    }

    /// <summary>Reprend la partie à la scène et l'étage sauvegardés.</summary>
    // Charge la save et navigue vers la scène correspondante
    public void OnContinueClicked()
    {
        // Ignore si aucune sauvegarde n'est disponible
        if (!SaveSystem.HasSave()) return;

        SaveSystem.LoadGame();

        // Charge la scène correspondant à l'étage sauvegardé
        SceneManager.LoadScene(GameProgress.Instance.GetCurrentSceneName());
    }

    /// <summary>Affiche le panneau des options et cache le menu.</summary>
    // Bascule l'affichage vers le panneau des options
    public void OnOptionsClicked()
    {
        _menuPanel.SetActive(false);
        _optionsPanel.SetActive(true);
    }
}
