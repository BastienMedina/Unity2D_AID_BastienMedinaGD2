using UnityEngine;
using UnityEngine.SceneManagement;

// Écoute la mort du joueur et retourne au menu principal
public class GameOver : MonoBehaviour
{
    // Nom de la scène du menu principal
    [SerializeField] private string _mainMenuScene = "Scene_MainMenu";

    // S'abonne à l'événement de mort du LivesManager
    private void OnEnable()
    {
        // Vérifie la présence du singleton avant de s'abonner
        LivesManager.Instance?.OnDeath.AddListener(HandleDeath);
    }

    // Se désabonne proprement à la désactivation du composant
    private void OnDisable()
    {
        // Retire le listener pour éviter les appels fantômes
        LivesManager.Instance?.OnDeath.RemoveListener(HandleDeath);
    }

    // Supprime la save (partie terminée), reset la progression et retourne au menu principal
    private void HandleDeath()
    {
        // Réinitialise la progression — la partie est perdue, pas sauvegardable
        if (GameProgress.Instance != null)
            GameProgress.Instance.Reset();

        // Supprime toute save existante pour désactiver le bouton Continuer au menu
        SaveSystem.DeleteSave();

        SceneManager.LoadScene(_mainMenuScene);
    }
}
