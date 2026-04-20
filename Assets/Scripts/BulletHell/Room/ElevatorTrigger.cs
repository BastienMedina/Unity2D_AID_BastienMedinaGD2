using UnityEngine;

// Ajouté dynamiquement par ProceduralMapGenerator sur la salle ascenseur.
// Déclenche la transition animée dès que le joueur entre dans la zone.
public class ElevatorTrigger : MonoBehaviour
{
    // Étage maximum géré par la scène Bullet Hell
    private const int BulletHellMaxFloor = 3;

    // Nom de la scène Bullet Hell rechargée pour un nouvel étage
    private const string BulletHellScene = "Scene_BulletHell";

    // Nom de la scène Game & Watch pour l'étage 4
    private const string GameAndWatchScene = "Scene_GameAndWatch";

    // Empêche un double déclenchement si le joueur reste dans le trigger
    private bool _triggered = false;

    // Déclenche la transition quand le joueur entre dans la zone de l'ascenseur
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (_triggered) return;

        _triggered = true;

        // Sauvegarde les vies courantes avant le changement de scène
        if (LivesManager.Instance != null && GameProgress.Instance != null)
            GameProgress.Instance.SaveLives(LivesManager.Instance.GetCurrentLives());

        // Calcule le prochain étage pour l'affichage de la transition
        int nextFloor = GameProgress.Instance.CurrentFloor + 1;
        string targetScene = nextFloor <= BulletHellMaxFloor ? BulletHellScene : GameAndWatchScene;

        // Sauvegarde et avance la progression avant le chargement
        GameProgress.Instance.AdvanceFloor();
        SaveSystem.SaveGame();

        FloorTransitionAnimator.Instance.TransitionToScene(targetScene, nextFloor);
    }
}
