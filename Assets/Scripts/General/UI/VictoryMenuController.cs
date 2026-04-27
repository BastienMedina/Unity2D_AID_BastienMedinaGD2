using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Affiche le menu de victoire. Appeler HandleVictory() depuis les systèmes de jeu.
public class VictoryMenuController : MonoBehaviour
{
    // Panneau racine du menu victoire
    [SerializeField] private GameObject _victoryPanel;

    // Nom de la scène du menu principal
    [SerializeField] private string _mainMenuScene = "Scene_MainMenu";

    // Son joué lors des interactions avec les boutons
    [SerializeField] private AudioClip _buttonClip;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    private void Awake()
    {
        _victoryPanel.SetActive(false);
    }

    private void Start()
    {
        // Câblage du bouton par code
        Button btnMainMenu = _victoryPanel.transform.Find("Panel/Button_MainMenu")?.GetComponent<Button>();
        btnMainMenu?.onClick.AddListener(OnMainMenuClicked);

        // Auto-branchement sur SI_ProgressionGauge si présent dans la scène (Space Invaders)
        // GameOverHandler prend en charge TurnManager (BossFight) directement via ses champs Inspector
        SI_ProgressionGauge gauge = FindFirstObjectByType<SI_ProgressionGauge>();
        if (gauge != null)
            gauge.OnVictory.AddListener(HandleVictory);
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
    // Déclenchement victoire — appeler depuis TurnManager.OnGameOver ou SI_ProgressionGauge.OnVictory
    // -------------------------------------------------------------------------

    /// <summary>Affiche le panneau de victoire et gèle le jeu.</summary>
    public void HandleVictory()
    {
        Time.timeScale = 0f;
        _victoryPanel.SetActive(true);
    }

    /// <summary>Surcharge booléenne pour TurnManager.OnGameOver(bool isVictory).</summary>
    public void HandleVictoryBool(bool isVictory)
    {
        if (isVictory)
            HandleVictory();
    }
}
