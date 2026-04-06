using UnityEngine;
using UnityEngine.SceneManagement;

// Gère la transition vers Space Invaders après le boss
public class BossDefeated : MonoBehaviour
{
    // Nom de la scène Space Invaders finale
    [SerializeField] private string _spaceInvadersScene = "Scene_SpaceInvaders";

    /// <summary>Appelé quand le boss du Game & Watch est vaincu.</summary>
    // Sauvegarde l'étage et charge la scène finale
    public void OnBossDefeated()
    {
        // Sauvegarde avant de passer au Space Invaders
        SaveSystem.SaveGame();
        GameProgress.Instance.AdvanceFloor();

        // Charge le mini-jeu Space Invaders final
        SceneManager.LoadScene(_spaceInvadersScene);
    }
}
