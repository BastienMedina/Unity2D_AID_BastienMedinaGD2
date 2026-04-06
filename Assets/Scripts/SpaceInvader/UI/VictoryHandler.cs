using UnityEngine;
using UnityEngine.SceneManagement;

// Écoute la victoire et efface la save avant le menu
public class VictoryHandler : MonoBehaviour
{
    // Nom de la scène du menu principal
    [SerializeField] private string _mainMenuScene = "Scene_MainMenu";

    // S'abonne à l'événement OnVictory de SI_ProgressionGauge
    private void OnEnable()
    {
        // Récupère la jauge sur le même GameObject pour s'abonner
        GetComponent<SI_ProgressionGauge>()?.OnVictory.AddListener(HandleVictory);
    }

    // Se désabonne proprement à la désactivation du composant
    private void OnDisable()
    {
        // Retire le listener pour éviter les appels fantômes
        GetComponent<SI_ProgressionGauge>()?.OnVictory.RemoveListener(HandleVictory);
    }

    // Supprime la save et retourne au menu après victoire
    private void HandleVictory()
    {
        // Supprime la sauvegarde : la partie est terminée avec succès
        SaveSystem.DeleteSave();

        // Retourne au menu principal après la victoire
        SceneManager.LoadScene(_mainMenuScene);
    }
}
