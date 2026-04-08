using System.Collections.Generic;
using UnityEngine;

// Singleton persistant gérant la progression par étage et l'inventaire inter-scènes
public class GameProgress : MonoBehaviour
{
    // Instance singleton accessible globalement
    public static GameProgress Instance { get; private set; }

    // Étage actuel du joueur entre 1 et 5
    public int CurrentFloor { get; private set; } = 1;

    // Inventaire persistant entre les scènes — null = slot vide
    private List<InventoryItem> _persistedInventory = new List<InventoryItem>();

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
        CurrentFloor = MinFloor;
        _persistedInventory.Clear();
    }

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
