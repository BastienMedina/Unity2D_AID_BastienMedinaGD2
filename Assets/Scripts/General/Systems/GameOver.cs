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

    // Sauvegarde l'étage atteint et retourne au menu principal
    private void HandleDeath()
    {
        // Sauvegarde l'étage atteint avant de mourir
        SaveSystem.SaveGame();
        SceneManager.LoadScene(_mainMenuScene);
    }
}
