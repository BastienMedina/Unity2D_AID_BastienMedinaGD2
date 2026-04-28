using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryHandler : MonoBehaviour
{
    [SerializeField] private string _mainMenuScene = "Scene_MainMenu";

    private void OnEnable()  // S'abonne à l'événement OnVictory de la jauge
    {
        GetComponent<SI_ProgressionGauge>()?.OnVictory.AddListener(HandleVictory);
    }

    private void OnDisable() // Se désabonne proprement pour éviter les appels fantômes
    {
        GetComponent<SI_ProgressionGauge>()?.OnVictory.RemoveListener(HandleVictory);
    }

    private void HandleVictory() // Supprime la save et retourne au menu principal
    {
        SaveSystem.DeleteSave();
        SceneManager.LoadScene(_mainMenuScene);
    }
}
