using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Affiche le menu de défaite lorsque LivesManager.OnDeath est déclenché.
public class GameOverMenuController : MonoBehaviour
{
    // Panneau racine du menu game over (l'Overlay plein-écran)
    [SerializeField] private GameObject _gameOverPanel;

    // Texte affichant l'étage atteint par le joueur
    [SerializeField] private TextMeshProUGUI _floorLabel;

    // Nom de la scène de départ (BulletHell étage 1)
    [SerializeField] private string _restartScene = "Scene_BulletHell";

    // Nom de la scène du menu principal
    [SerializeField] private string _mainMenuScene = "Scene_MainMenu";

    // Son joué lors des interactions avec les boutons
    [SerializeField] private AudioClip _buttonClip;

    // Image de l'overlay plein-écran — son raycastTarget est contrôlé pour ne pas bloquer l'UI sous-jacente
    private Image _overlayImage;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    private void Awake()
    {
        // Récupère l'Image de l'overlay pour contrôler son raycastTarget
        _overlayImage = _gameOverPanel.GetComponent<Image>();

        // Cache le panneau et désactive le raycast au démarrage
        _gameOverPanel.SetActive(false);
        SetOverlayRaycast(false);
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
        SetOverlayRaycast(true);
    }

    // Active ou désactive le raycast de l'overlay plein-écran
    private void SetOverlayRaycast(bool enabled)
    {
        if (_overlayImage != null)
            _overlayImage.raycastTarget = enabled;
    }
}
