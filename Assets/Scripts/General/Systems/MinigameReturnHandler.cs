using UnityEngine;
using UnityEngine.SceneManagement;

// À placer dans chaque scène de mini-jeu.
// En mode IsMinigameMode, intercepte la fin de partie (victoire ou game over)
// et retourne automatiquement au menu principal au lieu de progresser.
public class MinigameReturnHandler : MonoBehaviour
{
    // Nom de la scène du menu principal
    private const string SceneMainMenu = "Scene_MainMenu";

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    private void OnEnable()
    {
        // Ne fait rien si on n'est pas en mode mini-jeu
        if (!IsMinigame()) return;

        LivesManager.Instance?.OnDeath.AddListener(ReturnToMenu);
    }

    private void OnDisable()
    {
        LivesManager.Instance?.OnDeath.RemoveListener(ReturnToMenu);
    }

    // -------------------------------------------------------------------------
    // API publique
    // -------------------------------------------------------------------------

    /// <summary>Retourne au menu principal si on est en mode mini-jeu.
    /// À appeler depuis les handlers de victoire de chaque mini-jeu.</summary>
    public void ReturnToMenu()
    {
        if (!IsMinigame()) return;

        GameProgress.Instance?.Reset();
        SaveSystem.DeleteSave();
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneMainMenu);
    }

    // -------------------------------------------------------------------------
    // Interne
    // -------------------------------------------------------------------------

    private bool IsMinigame()
    {
        return GameProgress.Instance != null && GameProgress.Instance.IsMinigameMode;
    }
}
