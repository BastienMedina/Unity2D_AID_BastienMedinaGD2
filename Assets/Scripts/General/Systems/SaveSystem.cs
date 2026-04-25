using UnityEngine;

// Utilitaire statique pour la persistance de la progression
public static class SaveSystem
{
    // Clé PlayerPrefs stockant l'étage sauvegardé
    private const string SaveKey = "GameSave_Floor";

    // Clé PlayerPrefs stockant les vies sauvegardées
    private const string LivesKey = "GameSave_Lives";

    /// <summary>Retourne vrai si une sauvegarde existe.</summary>
    // Vérifie la présence de la clé dans PlayerPrefs
    public static bool HasSave()
        => PlayerPrefs.HasKey(SaveKey);

    /// <summary>Sauvegarde l'étage actuel et les vies courantes dans PlayerPrefs.</summary>
    // Écrit l'étage et les vies puis force la persistance disque
    public static void SaveGame()
    {
        PlayerPrefs.SetInt(SaveKey, GameProgress.Instance.CurrentFloor);

        // Sauvegarde les vies si le LivesManager est disponible
        if (LivesManager.Instance != null)
            PlayerPrefs.SetInt(LivesKey, LivesManager.Instance.GetCurrentLives());

        PlayerPrefs.Save();
    }

    /// <summary>Restaure la progression et les vies depuis la sauvegarde.</summary>
    // Définit directement l'étage et injecte les vies dans GameProgress
    public static void LoadGame()
    {
        int savedFloor = PlayerPrefs.GetInt(SaveKey, 1);
        GameProgress.Instance.SetFloor(savedFloor);

        // Restaure les vies via GameProgress pour qu'elles soient récupérées par LivesManager au Awake
        if (PlayerPrefs.HasKey(LivesKey))
        {
            int savedLives = PlayerPrefs.GetInt(LivesKey, 3);
            GameProgress.Instance.SaveLives(savedLives);
        }
    }

    /// <summary>Supprime la sauvegarde existante.</summary>
    // Efface les clés et force la persistance disque
    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.DeleteKey(LivesKey);
        PlayerPrefs.Save();
    }
}
