using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Écoute CoinSystem.OnVictoryConditionMet dans la scène Game and Watch.
/// Quand le boss est vaincu : avance l'étage, sauvegarde et charge Space Invaders.
/// </summary>
public class BossDefeated : MonoBehaviour
{
    // Référence directe au CoinSystem — à assigner dans l'Inspector
    [SerializeField] private CoinSystem _coinSystem;

    // Nom de la scène Space Invaders finale
    [SerializeField] private string _spaceInvadersScene = "Scene_SpaceInvaders";

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    private void Start()
    {
        if (_coinSystem == null)
            _coinSystem = FindFirstObjectByType<CoinSystem>();

        if (_coinSystem != null)
            _coinSystem.OnVictoryConditionMet.AddListener(OnBossDefeated);
        else
            Debug.LogError("[BossDefeated] CoinSystem introuvable dans la scène.", this);
    }

    private void OnDestroy()
    {
        if (_coinSystem != null)
            _coinSystem.OnVictoryConditionMet.RemoveListener(OnBossDefeated);
    }

    // -------------------------------------------------------------------------
    // Transition
    // -------------------------------------------------------------------------

    /// <summary>Appelé quand toutes les pièces sont collectées — charge Space Invaders.</summary>
    public void OnBossDefeated()
    {
        // Sauvegarde l'intégralité de la progression avant la transition
        FloorTransition.PersistFullState();

        GameProgress.Instance?.AdvanceFloor();

        SceneManager.LoadScene(_spaceInvadersScene);
    }
}
