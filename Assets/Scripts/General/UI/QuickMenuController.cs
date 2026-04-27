using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Bouton flottant toujours visible qui ouvre une popup de confirmation avant de quitter.
public class QuickMenuController : MonoBehaviour
{
    // Panneau de confirmation (popup)
    [SerializeField] private GameObject _confirmPanel;

    // Bouton flottant racine (le petit ≡ MENU en coin)
    [SerializeField] private Button _quickButton;

    // Nom de la scène du menu principal
    [SerializeField] private string _mainMenuScene = "Scene_MainMenu";

    // Son joué lors des interactions avec les boutons
    [SerializeField] private AudioClip _buttonClip;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    private void Awake()
    {
        _confirmPanel.SetActive(false);
    }

    private void Start()
    {
        // Câblage du bouton flottant par code
        if (_quickButton != null)
            _quickButton.onClick.AddListener(OnQuickMenuClicked);

        // Câblage des boutons de la popup
        Button btnConfirm = _confirmPanel.transform.Find("Panel/Button_Confirm")?.GetComponent<Button>();
        Button btnCancel  = _confirmPanel.transform.Find("Panel/Button_Cancel")?.GetComponent<Button>();

        btnConfirm?.onClick.AddListener(OnConfirmClicked);
        btnCancel?.onClick.AddListener(OnCancelClicked);
    }

    // -------------------------------------------------------------------------
    // Boutons
    // -------------------------------------------------------------------------

    /// <summary>Ouvre la popup de confirmation.</summary>
    public void OnQuickMenuClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        _confirmPanel.SetActive(true);
    }

    /// <summary>Sauvegarde et quitte vers le menu principal.</summary>
    public void OnConfirmClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        SaveSystem.SaveGame();
        Time.timeScale = 1f;
        SceneManager.LoadScene(_mainMenuScene);
    }

    /// <summary>Annule et ferme la popup.</summary>
    public void OnCancelClicked()
    {
        AudioManager.Instance?.PlaySFX(_buttonClip);
        _confirmPanel.SetActive(false);
    }
}
