using UnityEngine;

// Gère la transition d'étage depuis l'ascenseur
public class FloorTransition : MonoBehaviour
{
    // Étage limite pour rester dans la scène Bullet Hell
    [SerializeField] private int _bulletHellMaxFloor = 3;

    // Nom de la scène Bullet Hell procédurale
    [SerializeField] private string _bulletHellScene = "Scene_BulletHell";

    // Nom de la scène Game & Watch suivant le Bullet Hell
    [SerializeField] private string _gameAndWatchScene = "Scene_GameAndWatch";

    /// <summary>Appelé par ElevatorController._onFloorTransitionRequested.</summary>
    // Sauvegarde la progression et déclenche la transition animée vers l'étage suivant
    public void OnPlayerEnterElevator()
    {
        // Sauvegarde la progression avant de changer d'étage
        SaveSystem.SaveGame();
        GameProgress.Instance.AdvanceFloor();

        int nextFloor = GameProgress.Instance.CurrentFloor;
        string targetScene = nextFloor <= _bulletHellMaxFloor ? _bulletHellScene : _gameAndWatchScene;

        // Délègue le chargement au singleton animé
        FloorTransitionAnimator.Instance.TransitionToScene(targetScene, nextFloor);
    }
}
