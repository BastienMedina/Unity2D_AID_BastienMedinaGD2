using UnityEngine;
using UnityEngine.InputSystem;

// Écoute la touche Escape (ou le bouton Start/Menu manette) et bascule la pause.
// À placer sur GO_PauseManager ou sur un GO dédié à l'input dans la scène.
public class PauseInputHandler : MonoBehaviour
{
    // Référence au PauseManager de la scène
    private PauseManager _pauseManager;

    private void Awake()
    {
        _pauseManager = GetComponent<PauseManager>();

        if (_pauseManager == null)
            _pauseManager = FindFirstObjectByType<PauseManager>();

        if (_pauseManager == null)
            Debug.LogError("[PauseInputHandler] Aucun PauseManager trouvé dans la scène.", this);
    }

    private void Update()
    {
        // Keyboard.current peut être null sur certaines plateformes sans clavier
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            _pauseManager?.TogglePause();
            return;
        }

        // Support manette : bouton Start (Gamepad.startButton)
        if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
        {
            _pauseManager?.TogglePause();
        }
    }
}
