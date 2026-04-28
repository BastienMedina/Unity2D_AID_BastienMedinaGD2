using UnityEngine;

public static class SaveSystem
{
    private const string SaveKey  = "GameSave_Floor";
    private const string LivesKey = "GameSave_Lives";

    public static bool HasSave() => PlayerPrefs.HasKey(SaveKey); // Vérifie la présence de la clé

    public static void SaveGame() // Écrit l'étage et les vies dans PlayerPrefs
    {
        PlayerPrefs.SetInt(SaveKey, GameProgress.Instance.CurrentFloor);

        if (LivesManager.Instance != null)
            PlayerPrefs.SetInt(LivesKey, LivesManager.Instance.GetCurrentLives()); // Sauvegarde les vies

        PlayerPrefs.Save();
    }

    public static void LoadGame() // Restaure l'étage et les vies depuis la sauvegarde
    {
        int savedFloor = PlayerPrefs.GetInt(SaveKey, 1);
        GameProgress.Instance.SetFloor(savedFloor);

        if (PlayerPrefs.HasKey(LivesKey)) // Restaure les vies via GameProgress
        {
            int savedLives = PlayerPrefs.GetInt(LivesKey, 3);
            GameProgress.Instance.SaveLives(savedLives);
        }
    }

    public static void DeleteSave() // Efface toutes les clés et force la persistance disque
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.DeleteKey(LivesKey);
        PlayerPrefs.Save();
    }
}
