using System.Collections.Generic;
using UnityEngine;

public class FloorTransition : MonoBehaviour
{
    [SerializeField] private int _bulletHellMaxFloor = 3;
    [SerializeField] private string _bulletHellScene = "Scene_BulletHell";
    [SerializeField] private string _gameAndWatchScene = "Scene_GameAndWatch";

    public void OnPlayerEnterElevator() // Sauvegarde l'état et charge l'étage suivant
    {
        if (GameProgress.Instance == null)
        {
            Debug.LogError("[FloorTransition] GameProgress.Instance est null — transition annulée.", this);
            return;
        }

        PersistFullState();
        GameProgress.Instance.AdvanceFloor();

        int nextFloor      = GameProgress.Instance.CurrentFloor;
        string targetScene = nextFloor <= _bulletHellMaxFloor ? _bulletHellScene : _gameAndWatchScene;

        FloorTransitionAnimator.Instance.TransitionToScene(targetScene, nextFloor);
    }

    public static void PersistFullState() // Sauvegarde inventaire, buffs, vies et PV max
    {
        if (GameProgress.Instance == null) return;

        InventoryManager inv = FindFirstObjectByType<InventoryManager>();
        if (inv != null) // Sauvegarde l'inventaire complet
        {
            int capacity          = inv.GetCapacity();
            List<InventoryItem> items = new List<InventoryItem>(capacity);
            for (int i = 0; i < capacity; i++) // Parcourt tous les slots
                items.Add(inv.GetItem(i));
            GameProgress.Instance.SaveInventory(items);
        }
        else
        {
            Debug.LogWarning("[FloorTransition] InventoryManager introuvable — inventaire non sauvegardé.");
        }

        PlayerStatsManager stats = FindFirstObjectByType<PlayerStatsManager>();
        if (stats != null) // Sauvegarde les buffs et PV max de base
            GameProgress.Instance.SaveBuffs(stats.GetActiveBuffs(), stats.GetBaseMaxHealth());
        else
            Debug.LogWarning("[FloorTransition] PlayerStatsManager introuvable — buffs non sauvegardés.");

        if (LivesManager.Instance != null) // Sauvegarde vies courantes et max
        {
            GameProgress.Instance.SaveLives(LivesManager.Instance.GetCurrentLives());
            GameProgress.Instance.SaveMaxLives(LivesManager.Instance.GetMaxLives());
        }
        else
        {
            Debug.LogWarning("[FloorTransition] LivesManager introuvable — vies non sauvegardées.");
        }

        SaveSystem.SaveGame();
    }
}
