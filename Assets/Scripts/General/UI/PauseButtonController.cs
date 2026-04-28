using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bouton flottant présent dans chaque scène de jeu.
/// Au clic : bascule la pause via PauseManager et affiche / masque le PauseMenuController.
/// À placer sur le GameObject qui porte le composant Button du bouton pause.
/// </summary>
[RequireComponent(typeof(Button))]
public class PauseButtonController : MonoBehaviour
{
    // Référence au PauseMenuController de la scène — à assigner dans l'Inspector
    [SerializeField] private PauseMenuController _pauseMenuController;

    // Son joué au clic sur le bouton pause
    [SerializeField] private AudioClip _buttonClip;

    // Bouton Unity résolu automatiquement via RequireComponent
    private Button _button;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    private void Awake()
    {
        _button = GetComponent<Button>();

        if (_pauseMenuController == null)
            _pauseMenuController = FindFirstObjectByType<PauseMenuController>();

        if (_pauseMenuController == null)
            Debug.LogError("[PauseButtonController] PauseMenuController introuvable dans la scène.", this);
    }

    private void Start()
    {
        if (PauseManager.Instance == null)
            Debug.LogError("[PauseButtonController] PauseManager.Instance introuvable dans la scène.", this);

        _button.onClick.AddListener(OnPauseButtonClicked);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnPauseButtonClicked);
    }

    // -------------------------------------------------------------------------
    // Bouton
    // -------------------------------------------------------------------------

    /// <summary>Bascule la pause : ouvre ou ferme le menu pause.</summary>
    public void OnPauseButtonClicked()
    {
        if (PauseManager.Instance == null || _pauseMenuController == null)
            return;

        AudioManager.Instance?.PlaySFX(_buttonClip);

        if (PauseManager.Instance.IsPaused)
        {
            _pauseMenuController.Hide();
            PauseManager.Instance.Resume();
        }
        else
        {
            PauseManager.Instance.Pause();
            _pauseMenuController.Show();
        }
    }
}
