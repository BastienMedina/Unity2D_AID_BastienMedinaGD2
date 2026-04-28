using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameReturnHandler : MonoBehaviour
{
    private const string SceneMainMenu = "Scene_MainMenu";

    private void OnEnable() // S'abonne à OnDeath si en mode mini-jeu
    {
        if (!IsMinigame()) return;
        LivesManager.Instance?.OnDeath.AddListener(ReturnToMenu);
    }

    private void OnDisable() // Désabonne le listener de mort
    {
        LivesManager.Instance?.OnDeath.RemoveListener(ReturnToMenu);
    }

    /// <summary>Retourne au menu principal si on est en mode mini-jeu.</summary>
    public void ReturnToMenu() // Réinitialise et charge le menu principal
    {
        if (!IsMinigame()) return;

        GameProgress.Instance?.Reset();
        SaveSystem.DeleteSave();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneMainMenu);
    }

    private bool IsMinigame() => GameProgress.Instance != null && GameProgress.Instance.IsMinigameMode; // Vérifie le mode mini-jeu
}
