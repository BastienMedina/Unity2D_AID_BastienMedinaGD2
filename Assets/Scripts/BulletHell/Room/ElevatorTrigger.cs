using UnityEngine;

// Ajouté dynamiquement par ProceduralMapGenerator sur la salle ascenseur.
// Le timer démarre automatiquement au chargement de la scène (via ElevatorDoorController).
// La transition n'a lieu que quand le joueur entre dans l'ascenseur ET que le timer est écoulé.
public class ElevatorTrigger : MonoBehaviour
{
    // Référence au contrôleur de portes, résolu au démarrage.
    private ElevatorDoorController _doorController;

    // Anti double-trigger
    private bool _hasTriggered = false;

    private void Awake()
    {
        _doorController = GetComponent<ElevatorDoorController>();
    }

    // Déclenche la transition quand le joueur entre dans l'ascenseur et que le timer est écoulé.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        // Bloque la transition si le timer n'est pas encore terminé
        if (_doorController != null && !_doorController.IsTimerDone)
        {
            Debug.Log("[ElevatorTrigger] Timer pas encore écoulé — transition bloquée.");
            return;
        }

        _hasTriggered = true;
        TriggerFloorTransition();
    }

    // Sauvegarde l'état complet et charge l'étage suivant (ou retourne au menu en mode mini-jeu).
    private void TriggerFloorTransition()
    {
        if (GameProgress.Instance == null)
        {
            Debug.LogError("[ElevatorTrigger] GameProgress.Instance est null — transition annulée.", this);
            return;
        }

        // En mode mini-jeu : retour au menu sans progresser
        if (GameProgress.Instance.IsMinigameMode)
        {
            MinigameReturnHandler minigame = FindFirstObjectByType<MinigameReturnHandler>();
            if (minigame != null)
            {
                minigame.ReturnToMenu();
                return;
            }
        }

        // Sauvegarde inventaire, buffs, vies et PV max avant toute transition.
        FloorTransition.PersistFullState();

        // Incrémente l'étage.
        GameProgress.Instance.AdvanceFloor();

        int nextFloor      = GameProgress.Instance.CurrentFloor;
        string targetScene = GameProgress.Instance.GetCurrentSceneName();

        Debug.Log($"[ElevatorTrigger] Transition vers étage {nextFloor} → scène '{targetScene}'");

        // Utilise FloorTransitionAnimator s'il est disponible (fondu + label d'étage).
        if (FloorTransitionAnimator.Instance != null)
        {
            FloorTransitionAnimator.Instance.TransitionToScene(targetScene, nextFloor);
        }
        else
        {
            // Fallback direct si le singleton d'animation n'est pas chargé.
            Debug.LogWarning("[ElevatorTrigger] FloorTransitionAnimator introuvable — chargement direct.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
        }
    }
}
