using UnityEngine;
using UnityEngine.SceneManagement;

public class BossDefeated : MonoBehaviour
{
    [SerializeField] private CoinSystem _coinSystem;
    [SerializeField] private string _spaceInvadersScene = "Scene_SpaceInvaders";

    private void Start() // Cherche CoinSystem et s'abonne à la victoire
    {
        if (_coinSystem == null)
            _coinSystem = FindFirstObjectByType<CoinSystem>();

        if (_coinSystem != null)
            _coinSystem.OnVictoryConditionMet.AddListener(OnBossDefeated);
        else
            Debug.LogError("[BossDefeated] CoinSystem introuvable dans la scène.", this);
    }

    private void OnDestroy() // Désabonne l'écouteur de victoire
    {
        if (_coinSystem != null)
            _coinSystem.OnVictoryConditionMet.RemoveListener(OnBossDefeated);
    }

    public void OnBossDefeated() // Sauvegarde et charge Space Invaders
    {
        if (GameProgress.Instance != null && GameProgress.Instance.IsMinigameMode) // Mode mini-jeu : retour menu
        {
            MinigameReturnHandler minigame = FindFirstObjectByType<MinigameReturnHandler>();
            if (minigame != null)
            {
                minigame.ReturnToMenu();
                return;
            }
        }

        FloorTransition.PersistFullState(); // Sauvegarde avant transition
        GameProgress.Instance?.AdvanceFloor();
        SceneManager.LoadScene(_spaceInvadersScene);
    }
}
