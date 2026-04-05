using UnityEngine;
using UnityEngine.EventSystems;

// Diagnostique les deux bugs connus au démarrage de la scène
public class SI_DiagnosticShoot : MonoBehaviour
{
    // Lance tous les checks diagnostiques dès le démarrage
    private void Start()
    {
        // Vérifie que SI_Player existe dans la scène
        var player = FindObjectOfType<SI_PlayerShooter>();
        Debug.Log($"[SI] SI_PlayerShooter found = {player != null}");

        // Vérifie que le bouton Fire est câblé
        var btn = GameObject.Find("Button_Fire")
            ?.GetComponent<UnityEngine.UI.Button>();
        Debug.Log($"[SI] Button_Fire found = {btn != null}");

        // Compte les listeners sur le bouton Fire
        if (btn != null)
            Debug.Log($"[SI] Button_Fire onClick listeners = {btn.onClick.GetPersistentEventCount()}");

        // Vérifie l'EventSystem dans la scène
        var es = FindObjectOfType<EventSystem>();
        Debug.Log($"[SI] EventSystem found = {es != null}");

        // Vérifie que WaveManager existe dans la scène
        var wm = FindObjectOfType<SI_WaveManager>();
        Debug.Log($"[SI] SI_WaveManager found = {wm != null}");

        // Vérifie que les prefabs sont assignés sur WaveManager
        if (wm != null)
        {
            Debug.Log($"[SI] WaveManager._virusFastPrefab assigned = {wm.VirusFastPrefab != null}");
            Debug.Log($"[SI] WaveManager._virusHeavyPrefab assigned = {wm.VirusHeavyPrefab != null}");
            Debug.Log($"[SI] WaveManager._virusShooterPrefab assigned = {wm.VirusShooterPrefab != null}");
        }
    }
}
