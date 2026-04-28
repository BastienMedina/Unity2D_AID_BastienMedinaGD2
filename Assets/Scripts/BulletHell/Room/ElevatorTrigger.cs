using UnityEngine;

// Ajouté dynamiquement par ProceduralMapGenerator sur la salle ascenseur.
// Déclenche la sauvegarde complète et la transition vers l'étage suivant
// dès que toutes les salles requises sont libérées et que le joueur entre.
public class ElevatorTrigger : MonoBehaviour
{
    // Indique si la transition a déjà été déclenchée (anti double-trigger).
    private bool _hasTriggered = false;

    // Vérifie si toutes les salles de la scène sont libérées avant d'autoriser la transition.
    private bool AreAllRoomsCleared()
    {
        RoomController[] rooms = FindObjectsByType<RoomController>(FindObjectsSortMode.None);

        // Aucune salle trouvée = pas de condition de blocage, on laisse passer.
        if (rooms == null || rooms.Length == 0)
            return true;

        foreach (RoomController room in rooms)
        {
            if (room != null && !room.IsCleared())
                return false;
        }

        return true;
    }

    // Déclenche la transition quand le joueur entre dans la zone de l'ascenseur.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        if (!AreAllRoomsCleared())
        {
            Debug.Log("[ElevatorTrigger] Des salles ne sont pas encore libérées — transition bloquée.");
            return;
        }

        _hasTriggered = true;
        TriggerFloorTransition();
    }

    // Sauvegarde l'état complet et charge l'étage suivant.
    private void TriggerFloorTransition()
    {
        if (GameProgress.Instance == null)
        {
            Debug.LogError("[ElevatorTrigger] GameProgress.Instance est null — transition annulée.", this);
            return;
        }

        // Sauvegarde inventaire, buffs, vies et PV max avant toute transition.
        FloorTransition.PersistFullState();

        // Incrémente l'étage.
        GameProgress.Instance.AdvanceFloor();

        int nextFloor = GameProgress.Instance.CurrentFloor;
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
