using UnityEngine;
using UnityEngine.SceneManagement;

// Gère la transition vers Space Invaders après le boss
public class BossDefeated : MonoBehaviour
{
    // Nom de la scène Space Invaders finale
    [SerializeField] private string _spaceInvadersScene = "Scene_SpaceInvaders";

    // S'abonne à la victoire du CoinSystem au démarrage
    private void Start()
    {
        // Écoute la victoire du Game & Watch via CoinSystem
        CoinSystem coinSystem = FindObjectOfType<CoinSystem>();

        if (coinSystem != null)
            coinSystem.OnVictoryConditionMet.AddListener(OnBossDefeated);
        else
            Debug.LogWarning("[BossDefeated] Aucun CoinSystem trouvé dans la scène.", this);
    }

    // Se désabonne proprement à la désactivation du composant
    private void OnDestroy()
    {
        // Retire le listener pour éviter les appels fantômes
        CoinSystem coinSystem = FindObjectOfType<CoinSystem>();
        if (coinSystem != null)
            coinSystem.OnVictoryConditionMet.RemoveListener(OnBossDefeated);
    }

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
