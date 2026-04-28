using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Affiche l'écran de victoire à la fin de Space Invaders uniquement.
/// Se branche automatiquement sur SI_ProgressionGauge.OnVictory dans Start().
/// Gèle le jeu via PauseManager.LockForEndState() pour bloquer toute pause ultérieure.
/// À placer sur le Canvas victoire dans la scène Scene_SpaceInvaders.
/// </summary>
public class VictoryMenuController : MonoBehaviour
{
    // Panneau racine du menu victoire
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
        if (_panel != null)
            _panel.SetActive(false);
    }

    private void Start()
    {
        // Branchement automatique sur SI_ProgressionGauge (Space Invaders)
        SI_ProgressionGauge gauge = FindFirstObjectByType<SI_ProgressionGauge>();
        if (gauge != null)
        {
            gauge.OnVictory.AddListener(HandleVictory);
        }
        else
        {
            Debug.LogWarning("[VictoryMenuController] SI_ProgressionGauge introuvable — la victoire ne se déclenchera pas.", this);
        }

        Button btnMainMenu = FindButtonInChildren(_panel.transform, "Button_MainMenu");
        btnMainMenu?.onClick.AddListener(OnMainMenuClicked);
    }

    // -------------------------------------------------------------------------
    // Déclenchement victoire
    // -------------------------------------------------------------------------

    /// <summary>Appelé par SI_ProgressionGauge.OnVictory — affiche le panneau et gèle le jeu.</summary>
    public void HandleVictory()
    {
        PauseManager.Instance?.LockForEndState();
        _panel.SetActive(true);
    }

    // -------------------------------------------------------------------------
    // Boutons
    // -------------------------------------------------------------------------

    /// <summary>Efface la save et retourne au menu principal.</summary>
    public void OnMainMenuClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        SaveSystem.DeleteSave();
        GameProgress.Instance?.Reset();
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
