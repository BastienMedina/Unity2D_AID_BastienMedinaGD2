using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Affiche le menu de défaite lorsque LivesManager.OnDeath est déclenché.
public class GameOverMenuController : MonoBehaviour
{
    // Panneau racine du menu game over
    [SerializeField] private GameObject _gameOverPanel;

    // Texte affichant l'étage atteint par le joueur
    [SerializeField] private TextMeshProUGUI _floorLabel;

    // Nom de la scène de départ (BulletHell étage 1)
    [SerializeField] private string _restartScene = "Scene_BulletHell";

    // Nom de la scène du menu principal
    [SerializeField] private string _mainMenuScene = "Scene_MainMenu";

    // Son joué lors des interactions avec les boutons
    [SerializeField] private AudioClip _buttonClip;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    private void Awake()
    {
        _gameOverPanel.SetActive(false);
    }

    private void Start()
    {
        // Câblage des boutons par code
        Button btnRestart  = _gameOverPanel.transform.Find("Panel/Button_Restart")?.GetComponent<Button>();
        Button btnMainMenu = _gameOverPanel.transform.Find("Panel/Button_MainMenu")?.GetComponent<Button>();

        btnRestart?.onClick.AddListener(OnRestartClicked);
        btnMainMenu?.onClick.AddListener(OnMainMenuClicked);

        // Abonnement à OnDeath ici : Start() garantit que tous les Awake() ont déjà été exécutés,
        // donc LivesManager.Instance est forcément initialisé à ce stade.
        if (LivesManager.Instance != null)
            LivesManager.Instance.OnDeath.AddListener(HandleDeath);
        else
            Debug.LogError("[GameOverMenuController] LivesManager.Instance est null dans Start() — le menu game over ne s'affichera pas.", this);
    }

    private void OnDestroy()
    {
        // Nettoyage de l'abonnement pour éviter les références fantômes
        LivesManager.Instance?.OnDeath.RemoveListener(HandleDeath);
    }

    // -------------------------------------------------------------------------
    // Boutons
    // -------------------------------------------------------------------------

    /// <summary>Repart depuis l'étage 1 sans sauvegarde.</summary>
    public void OnRestartClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        GameProgress.Instance?.Reset();
        SaveSystem.DeleteSave();
        Time.timeScale = 1f;
        SceneManager.LoadScene(_restartScene);
    }

    /// <summary>Retourne au menu principal sans sauvegarder.</summary>
    public void OnMainMenuClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        GameProgress.Instance?.Reset();
        SaveSystem.DeleteSave();
        Time.timeScale = 1f;
        SceneManager.LoadScene(_mainMenuScene);
    }

    // -------------------------------------------------------------------------
    // Déclenchement mort — appelable aussi depuis GameOverHandler
    // -------------------------------------------------------------------------

    /// <summary>Affiche le panneau game over et gèle le jeu.</summary>
    public void HandleDeath()
    {
        if (_floorLabel != null && GameProgress.Instance != null)
            _floorLabel.text = "ÉTAGE " + GameProgress.Instance.CurrentFloor;

        Time.timeScale = 0f;
        _gameOverPanel.SetActive(true);
    }
}
