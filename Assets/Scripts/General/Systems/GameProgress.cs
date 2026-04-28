using System.Collections.Generic;
using UnityEngine;

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance { get; private set; }

    public int  CurrentFloor    { get; private set; } = 1;
    public bool IsMinigameMode  { get; private set; } = false;

    private List<InventoryItem> _persistedInventory  = new List<InventoryItem>();
    private int _persistedLives                      = -1;
    private int _persistedMaxLives                   = -1;
    private List<ActiveBuff> _persistedBuffs         = new List<ActiveBuff>();
    private int _persistedBaseMaxHealth              = -1;

    public bool HasPersistedLives    => _persistedLives    >= 0;
    public bool HasPersistedMaxLives => _persistedMaxLives >= 0;
    public bool HasPersistedBuffs    => _persistedBuffs.Count > 0 || _persistedBaseMaxHealth >= 0;

    private const int MinFloor = 1;
    private const int MaxFloor = 5;

    private const string SceneBulletHell    = "Scene_BulletHell";
    private const string SceneGameAndWatch  = "Scene_GameAndWatch";
    private const string SceneSpaceInvaders = "Scene_SpaceInvaders";
    private const string SceneMainMenu      = "Scene_MainMenu";

    private void Awake() // Initialise le singleton persistant
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AdvanceFloor() => CurrentFloor = Mathf.Min(CurrentFloor + 1, MaxFloor); // Incrémente sans dépasser le max

    public void SetFloor(int floor) => CurrentFloor = Mathf.Clamp(floor, MinFloor, MaxFloor); // Clamp entre min et max

    public void Reset() // Réinitialise l'étage, le mode mini-jeu et tous les persistés
    {
        CurrentFloor            = MinFloor;
        IsMinigameMode          = false;
        _persistedInventory.Clear();
        _persistedLives         = -1;
        _persistedMaxLives      = -1;
        _persistedBuffs.Clear();
        _persistedBaseMaxHealth = -1;
    }

    public void SetMinigameMode(int floor) // Active IsMinigameMode et définit l'étage
    {
        IsMinigameMode = true;
        SetFloor(floor);
    }

    public void SaveBuffs(IReadOnlyList<ActiveBuff> buffs, int baseMaxHealth) // Sauvegarde buffs et PV max de base
    {
        _persistedBuffs         = new List<ActiveBuff>(buffs);
        _persistedBaseMaxHealth = baseMaxHealth;
    }

    public List<ActiveBuff> PopBuffs() // Copie et vide la liste des buffs
    {
        List<ActiveBuff> copy = new List<ActiveBuff>(_persistedBuffs);
        _persistedBuffs.Clear();
        return copy;
    }

    public int PopBaseMaxHealth() // Retourne et remet à -1 la valeur persistée
    {
        int value               = _persistedBaseMaxHealth;
        _persistedBaseMaxHealth = -1;
        return value;
    }

    public void SaveInventory(IReadOnlyList<InventoryItem> slots) => _persistedInventory = new List<InventoryItem>(slots); // Copie la liste

    public List<InventoryItem> PopInventory() // Copie et vide l'inventaire persisté
    {
        List<InventoryItem> copy = new List<InventoryItem>(_persistedInventory);
        _persistedInventory.Clear();
        return copy;
    }

    public bool HasPersistedInventory() => _persistedInventory.Count > 0; // Vérifie si un inventaire est persisté

    public void SaveLives(int currentLives) => _persistedLives = currentLives; // Sauvegarde les vies courantes

    public void SaveMaxLives(int maxLives) => _persistedMaxLives = maxLives; // Sauvegarde le PV max courant

    public int PopLives() // Retourne et remet à -1 la valeur persistée
    {
        int lives       = _persistedLives;
        _persistedLives = -1;
        return lives;
    }

    public int PopMaxLives() // Retourne et remet à -1 la valeur persistée
    {
        int maxLives       = _persistedMaxLives;
        _persistedMaxLives = -1;
        return maxLives;
    }

    public string GetCurrentSceneName() // Mappe l'étage courant à son nom de scène
    {
        return CurrentFloor switch
        {
            1 => SceneBulletHell,
            2 => SceneBulletHell,
            3 => SceneBulletHell,
            4 => SceneGameAndWatch,
            5 => SceneSpaceInvaders,
            _ => SceneMainMenu
        };
    }
}
