using UnityEngine;
using UnityEngine.SceneManagement;

// Détecte l'entrée joueur et déclenche la transition d'étage
public class ElevatorTrigger : MonoBehaviour
{
    // Étage maximum géré par la scène Bullet Hell
    [SerializeField] private int _bulletHellMaxFloor = 3;

    // Nom de la scène Bullet Hell rechargée pour un nouvel étage
    [SerializeField] private string _bulletHellScene = "Scene_BulletHell";

    // Nom de la scène Game & Watch pour l'étage 4
    [SerializeField] private string _gameAndWatchScene = "Scene_GameAndWatch";

    // Détecte l'entrée du joueur dans l'ascenseur
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore les collisions qui ne proviennent pas du joueur
        if (!other.CompareTag("Player")) return;

        // Sauvegarde avant la transition d'étage
        SaveSystem.SaveGame();
        GameProgress.Instance.AdvanceFloor();

        int floor = GameProgress.Instance.CurrentFloor;

        if (floor <= _bulletHellMaxFloor)
        {
            // Recharge la même scène : nouvel étage procédural
            SceneManager.LoadScene(_bulletHellScene);
        }
        else
        {
            // Étage 4 : combat de boss Game & Watch
            SceneManager.LoadScene(_gameAndWatchScene);
        }
    }
}
