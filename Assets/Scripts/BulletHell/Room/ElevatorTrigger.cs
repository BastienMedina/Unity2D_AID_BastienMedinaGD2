using UnityEngine;

public class ElevatorTrigger : MonoBehaviour
{
    private ElevatorDoorController _doorController;
    private bool _hasTriggered = false;

    private void Awake() // Récupère le controller de portes
    {
        _doorController = GetComponent<ElevatorDoorController>();
    }

    private void OnTriggerEnter2D(Collider2D other) // Déclenche la transition si conditions remplies
    {
        if (_hasTriggered || !other.CompareTag("Player")) return;

        if (_doorController != null && !_doorController.IsTimerDone) // Bloque si timer pas terminé
            return;

        _hasTriggered = true;
        TriggerFloorTransition();
    }

    private void TriggerFloorTransition() // Sauvegarde l'état et charge l'étage suivant
    {
        if (GameProgress.Instance == null)
        {
            Debug.LogError("[ElevatorTrigger] GameProgress.Instance est null — transition annulée.", this);
            return;
        }

        if (GameProgress.Instance.IsMinigameMode) // Retour au menu en mode mini-jeu
        {
            MinigameReturnHandler minigame = FindFirstObjectByType<MinigameReturnHandler>();
            if (minigame != null) { minigame.ReturnToMenu(); return; }
        }

        FloorTransition.PersistFullState();
        GameProgress.Instance.AdvanceFloor();

        int nextFloor      = GameProgress.Instance.CurrentFloor;
        string targetScene = GameProgress.Instance.GetCurrentSceneName();

        if (FloorTransitionAnimator.Instance != null)
            FloorTransitionAnimator.Instance.TransitionToScene(targetScene, nextFloor);
        else
        {
            Debug.LogWarning("[ElevatorTrigger] FloorTransitionAnimator introuvable — chargement direct.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
        }
    }
}
