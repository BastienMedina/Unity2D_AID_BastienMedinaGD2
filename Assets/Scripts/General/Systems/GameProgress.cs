using System.Collections.Generic;
using UnityEngine;

// Singleton persistant gérant la progression par étage et l'inventaire inter-scènes
public class GameProgress : MonoBehaviour
{
    // Instance singleton accessible globalement
    public static GameProgress Instance { get; private set; }

    // Étage actuel du joueur entre 1 et 5
    public int CurrentFloor { get; private set; } = 1;

    /// <summary>Vrai quand le joueur lance un mini-jeu depuis le menu "All Games".
    /// Dans ce mode, la fin de partie retourne toujours au menu principal.</summary>
    public bool IsMinigameMode { get; private set; } = false;

    // Inventaire persistant entre les scènes — null = slot vide
    private List<InventoryItem> _persistedInventory = new List<InventoryItem>();

    // Vies persistées entre les scènes — -1 = non initialisé
    private int _persistedLives = -1;

    // PV max persistés entre les scènes — -1 = non initialisé
    private int _persistedMaxLives = -1;

    // Buffs actifs persistés entre les scènes
    private List<ActiveBuff> _persistedBuffs = new List<ActiveBuff>();

    // PV max de base persistés entre les scènes — -1 = non initialisé
    private int _persistedBaseMaxHealth = -1;

    /// <summary>Vrai si des vies ont été sauvegardées et attendent d'être restaurées.</summary>
    public bool HasPersistedLives => _persistedLives >= 0;

    /// <summary>Vrai si un PV max a été sauvegardé et attend d'être restauré.</summary>
    public bool HasPersistedMaxLives => _persistedMaxLives >= 0;

    // Étage minimum possible dans le jeu
    private const int MinFloor = 1;

    // Étage maximum possible dans le jeu
    private const int MaxFloor = 5;

    // Nom de la scène Bullet Hell procédurale
    private const string SceneBulletHell = "Scene_BulletHell";

    // Nom de la scène Game & Watch boss
    private const string SceneGameAndWatch = "Scene_GameAndWatch";

    // Nom de la scène Space Invaders finale
    private const string SceneSpaceInvaders = "Scene_SpaceInvaders";

    // Nom de la scène du menu principal
    private const string SceneMainMenu = "Scene_MainMenu";

    // Initialise le singleton et le rend persistant entre scènes
    private void Awake()
    {
        // Détruit le doublon si un singleton existe déjà
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Enregistre cette instance comme référence globale
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>Avance le joueur à l'étage suivant, plafonné à MaxFloor.</summary>
    // Incrémente l'étage courant sans dépasser le maximum
    public void AdvanceFloor()
    {
        CurrentFloor = Mathf.Min(CurrentFloor + 1, MaxFloor);
    }

    /// <summary>Définit directement l'étage sans incrémenter.</summary>
    // Restaure l'étage depuis la sauvegarde sans boucler
    public void SetFloor(int floor)
    {
        CurrentFloor = Mathf.Clamp(floor, MinFloor, MaxFloor);
    }

    /// <summary>Remet la progression à l'étage de départ et vide l'inventaire persisté.</summary>
    // Réinitialise le compteur d'étage à un et efface l'inventaire sauvegardé
    public void Reset()
    {
        CurrentFloor    = MinFloor;
        IsMinigameMode  = false;
        _persistedInventory.Clear();
        _persistedLives = -1;
        _persistedMaxLives = -1;
        _persistedBuffs.Clear();
        _persistedBaseMaxHealth = -1;
    }

    /// <summary>Active le mode mini-jeu isolé et fixe l'étage cible.</summary>
    public void SetMinigameMode(int floor)
    {
        IsMinigameMode = true;
        SetFloor(floor);
    }

    // -----------------------------------------------------------------------
    // API buffs persistants
    // -----------------------------------------------------------------------

    /// <summary>Sauvegarde les buffs actifs et les PV max de base pour la prochaine scène.</summary>
    public void SaveBuffs(IReadOnlyList<ActiveBuff> buffs, int baseMaxHealth)
    {
        _persistedBuffs = new List<ActiveBuff>(buffs);
        _persistedBaseMaxHealth = baseMaxHealth;
    }

    /// <summary>Retourne les buffs persistés et les vide pour éviter une double-restauration.</summary>
    public List<ActiveBuff> PopBuffs()
    {
        List<ActiveBuff> copy = new List<ActiveBuff>(_persistedBuffs);
        _persistedBuffs.Clear();
        return copy;
    }

    /// <summary>Retourne les PV max de base persistés et réinitialise le slot.</summary>
    public int PopBaseMaxHealth()
    {
        int value = _persistedBaseMaxHealth;
        _persistedBaseMaxHealth = -1;
        return value;
    }

    /// <summary>Vrai si des buffs ont été sauvegardés et attendent d'être restaurés.</summary>
    public bool HasPersistedBuffs => _persistedBuffs.Count > 0 || _persistedBaseMaxHealth >= 0;

    // -----------------------------------------------------------------------
    // API inventaire persistant
    // -----------------------------------------------------------------------

    /// <summary>Sauvegarde une copie de l'inventaire courant pour le prochain chargement de scène.</summary>
    public void SaveInventory(IReadOnlyList<InventoryItem> slots)
    {
        _persistedInventory = new List<InventoryItem>(slots);
    }

    /// <summary>Retourne l'inventaire persisté et le vide pour éviter une double-restauration.</summary>
    public List<InventoryItem> PopInventory()
    {
        List<InventoryItem> copy = new List<InventoryItem>(_persistedInventory);
        _persistedInventory.Clear();
        return copy;
    }

    /// <summary>Retourne vrai si un inventaire a été sauvegardé et attend d'être restauré.</summary>
    public bool HasPersistedInventory() => _persistedInventory.Count > 0;

    // -----------------------------------------------------------------------
    // API vies persistantes
    // -----------------------------------------------------------------------

    /// <summary>Sauvegarde les vies courantes pour la prochaine scène.</summary>
    public void SaveLives(int currentLives)
    {
        _persistedLives = currentLives;
    }

    /// <summary>Sauvegarde le PV max courant pour la prochaine scène.</summary>
    public void SaveMaxLives(int maxLives)
    {
        _persistedMaxLives = maxLives;
    }

    /// <summary>Retourne les vies persistées et réinitialise le slot.</summary>
    public int PopLives()
    {
        int lives = _persistedLives;
        _persistedLives = -1;
        return lives;
    }

    /// <summary>Retourne le PV max persisté et réinitialise le slot.</summary>
    public int PopMaxLives()
    {
        int maxLives = _persistedMaxLives;
        _persistedMaxLives = -1;
        return maxLives;
    }

    /// <summary>Retourne le nom de scène correspondant à l'étage actuel.</summary>
    // Mappe l'étage courant à son nom de scène
    public string GetCurrentSceneName()
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
