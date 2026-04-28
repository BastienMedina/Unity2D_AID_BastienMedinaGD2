using System.Collections.Generic;
using UnityEngine;

// Gère la transition d'étage depuis l'ascenseur.
// Point central unique de sauvegarde : inventaire, buffs, vies et PV max.
public class FloorTransition : MonoBehaviour
{
    // Étage limite pour rester dans la scène Bullet Hell
    [SerializeField] private int _bulletHellMaxFloor = 3;

    // Nom de la scène Bullet Hell procédurale
    [SerializeField] private string _bulletHellScene = "Scene_BulletHell";

    // Nom de la scène Game & Watch suivant le Bullet Hell
    [SerializeField] private string _gameAndWatchScene = "Scene_GameAndWatch";

    /// <summary>
    /// Appelé par ElevatorController._onFloorTransitionRequested.
    /// Sauvegarde l'intégralité de la progression avant le changement de scène.
    /// </summary>
    public void OnPlayerEnterElevator()
    {
        if (GameProgress.Instance == null)
        {
            Debug.LogError("[FloorTransition] GameProgress.Instance est null — transition annulée.", this);
            return;
        }

        PersistFullState();

        GameProgress.Instance.AdvanceFloor();

        int nextFloor = GameProgress.Instance.CurrentFloor;
        string targetScene = nextFloor <= _bulletHellMaxFloor ? _bulletHellScene : _gameAndWatchScene;

        // Délègue le chargement animé au singleton dédié
        FloorTransitionAnimator.Instance.TransitionToScene(targetScene, nextFloor);
    }

    /// <summary>
    /// Sauvegarde l'inventaire, les buffs, les PV max et les vies dans GameProgress,
    /// puis persiste l'étage et les vies dans PlayerPrefs via SaveSystem.
    /// Peut être appelé depuis d'autres scripts de transition (BossDefeated, etc.).
    /// </summary>
    public static void PersistFullState()
    {
        if (GameProgress.Instance == null)
            return;

        // Sauvegarde l'inventaire
        InventoryManager inv = FindFirstObjectByType<InventoryManager>();
        if (inv != null)
        {
            int capacity = inv.GetCapacity();
            List<InventoryItem> items = new List<InventoryItem>(capacity);
            for (int i = 0; i < capacity; i++)
                items.Add(inv.GetItem(i));

            GameProgress.Instance.SaveInventory(items);
            Debug.Log("[FloorTransition] Inventaire sauvegardé.");
        }
        else
        {
            Debug.LogWarning("[FloorTransition] InventoryManager introuvable — inventaire non sauvegardé.");
        }

        // Sauvegarde les buffs actifs et les PV max de base
        PlayerStatsManager stats = FindFirstObjectByType<PlayerStatsManager>();
        if (stats != null)
        {
            GameProgress.Instance.SaveBuffs(stats.GetActiveBuffs(), stats.GetBaseMaxHealth());
            Debug.Log($"[FloorTransition] Buffs sauvegardés. PV max base : {stats.GetBaseMaxHealth()}");
        }
        else
        {
            Debug.LogWarning("[FloorTransition] PlayerStatsManager introuvable — buffs non sauvegardés.");
        }

        // Sauvegarde les vies courantes et le PV max dans GameProgress
        if (LivesManager.Instance != null)
        {
            GameProgress.Instance.SaveLives(LivesManager.Instance.GetCurrentLives());
            GameProgress.Instance.SaveMaxLives(LivesManager.Instance.GetMaxLives());
            Debug.Log($"[FloorTransition] Vies sauvegardées : " +
                $"{LivesManager.Instance.GetCurrentLives()} / {LivesManager.Instance.GetMaxLives()}");
        }
        else
        {
            Debug.LogWarning("[FloorTransition] LivesManager introuvable — vies non sauvegardées.");
        }

        // Persiste étage et vies sur disque via PlayerPrefs
        SaveSystem.SaveGame();
    }
}
