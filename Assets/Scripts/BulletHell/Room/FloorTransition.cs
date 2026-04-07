using UnityEngine;
using UnityEngine.SceneManagement;

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
    // Sauvegarde et charge la scène du prochain étage
    public void OnPlayerEnterElevator()
    {
        // Sauvegarde la progression avant de changer d'étage
        SaveSystem.SaveGame();
        GameProgress.Instance.AdvanceFloor();

        int nextFloor = GameProgress.Instance.CurrentFloor;

        if (nextFloor <= _bulletHellMaxFloor)
        {
            // Recharge la même scène pour générer un nouvel étage
            SceneManager.LoadScene(_bulletHellScene);
        }
        else
        {
            // Charge le mini-jeu Game & Watch à l'étage 4
            SceneManager.LoadScene(_gameAndWatchScene);
        }
    }
}
