using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PauseButtonController : MonoBehaviour
{
    [SerializeField] private PauseMenuController _pauseMenuController;
    [SerializeField] private AudioClip _buttonClip;

    private Button _button;

    private void Awake() // Résout le bouton et le PauseMenuController
    {
        _button = GetComponent<Button>();
        if (_pauseMenuController == null)
            _pauseMenuController = FindFirstObjectByType<PauseMenuController>();
    }

    private void Start() // Câble le listener du bouton pause
    {
        _button.onClick.AddListener(OnPauseButtonClicked);
    }

    private void OnDestroy() // Désabonne le listener du bouton
    {
        _button.onClick.RemoveListener(OnPauseButtonClicked);
    }

    public void OnPauseButtonClicked() // Alterne Show/Hide selon l'état courant
    {
        if (PauseManager.Instance == null || _pauseMenuController == null) return;
        AudioManager.Instance?.PlaySFX(_buttonClip);

        if (PauseManager.Instance.IsPaused) { _pauseMenuController.Hide(); PauseManager.Instance.Resume(); }
        else                                { PauseManager.Instance.Pause(); _pauseMenuController.Show(); }
    }
}
