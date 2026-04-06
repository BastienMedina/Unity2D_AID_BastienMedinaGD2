using UnityEngine;

// Singleton persistant gérant la progression par étage
public class GameProgress : MonoBehaviour
{
    // Instance singleton accessible globalement
    public static GameProgress Instance { get; private set; }

    // Étage actuel du joueur entre 1 et 3
    public int CurrentFloor { get; private set; } = 1;

    // Nombre minimum d'étages jouables
    private const int MinFloor = 1;

    // Nom de la scène Bullet Hell procédurale
    private const string SceneBulletHell = "Scene_BulletHell";

    // Nom de la scène Game & Watch boss
    private const string SceneGameAndWatch = "Scene_GameAndWatch";

    // Nom de la scène Space Invaders finale
    private const string SceneSpaceInvaders = "Scene_SpaceInvaders";

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

    /// <summary>Avance le joueur à l'étage suivant.</summary>
    // Incrémente l'étage courant de un
    public void AdvanceFloor()
    {
        CurrentFloor++;
    }

    /// <summary>Remet la progression à l'étage de départ.</summary>
    // Réinitialise le compteur d'étage à un
    public void Reset()
    {
        CurrentFloor = MinFloor;
    }

    /// <summary>Retourne le nom de scène correspondant à l'étage actuel.</summary>
    // Mappe l'étage courant à son nom de scène
    public string GetCurrentSceneName()
    {
        return CurrentFloor switch
        {
            1 => SceneBulletHell,
            2 => SceneBulletHell,
            3 => SceneGameAndWatch,
            _ => SceneSpaceInvaders
        };
    }
}
