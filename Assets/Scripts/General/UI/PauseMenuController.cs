using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Contrôle l'affichage du panneau pause.
/// S'abonne au PauseManager pour afficher / masquer le panneau.
/// Câble les boutons Reprendre et Menu principal par code.
/// À placer sur le Canvas du menu pause dans chaque scène de jeu.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    // Panneau racine du menu pause (contient le titre et les boutons)
    [SerializeField] private GameObject _panel;

    // Nom de la scène du menu principal
    [SerializeField] private string _mainMenuScene = "Scene_MainMenu";

    // Son joué lors des interactions avec les boutons
    [SerializeField] private AudioClip _buttonClip;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    private void Awake()
    {
        // Cache le panneau au démarrage sans attendre le PauseManager
        if (_panel != null)
            _panel.SetActive(false);
    }

    private void Start()
    {
        if (PauseManager.Instance == null)
        {
            Debug.LogError("[PauseMenuController] PauseManager.Instance introuvable dans la scène.", this);
            return;
        }

        // Câblage des boutons par code — les noms doivent correspondre exactement au prefab
        Button btnResume   = FindButtonInChildren(_panel.transform, "Button_Resume");
        Button btnMainMenu = FindButtonInChildren(_panel.transform, "Button_MainMenu");

        btnResume?.onClick.AddListener(OnResumeClicked);
        btnMainMenu?.onClick.AddListener(OnMainMenuClicked);
    }

    // -------------------------------------------------------------------------
    // API publique — appelées par PauseManager ou par d'autres systèmes
    // -------------------------------------------------------------------------

    /// <summary>Affiche le panneau pause.</summary>
    public void Show()
    {
        if (_panel != null)
            _panel.SetActive(true);
    }

    /// <summary>Masque le panneau pause.</summary>
    public void Hide()
    {
        if (_panel != null)
            _panel.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Boutons
    // -------------------------------------------------------------------------

    /// <summary>Reprend le jeu et ferme le menu pause.</summary>
    public void OnResumeClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        Hide();
        PauseManager.Instance?.Resume();
    }

    /// <summary>Sauvegarde, restaure le timeScale et retourne au menu principal.</summary>
    public void OnMainMenuClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        SaveSystem.SaveGame();

        // On passe par Resume pour synchroniser l'état interne du PauseManager
        PauseManager.Instance?.Resume();

        SceneManager.LoadScene(_mainMenuScene);
    }

    // -------------------------------------------------------------------------
    // Utilitaire
    // -------------------------------------------------------------------------

    // Cherche récursivement un bouton par nom dans toute la hiérarchie enfant
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
