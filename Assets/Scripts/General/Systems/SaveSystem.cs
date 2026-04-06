using UnityEngine;

// Utilitaire statique pour la persistance de la progression
public static class SaveSystem
{
    // Clé PlayerPrefs stockant l'étage sauvegardé
    private const string SaveKey = "GameSave_Floor";

    /// <summary>Retourne vrai si une sauvegarde existe.</summary>
    // Vérifie la présence de la clé dans PlayerPrefs
    public static bool HasSave()
        => PlayerPrefs.HasKey(SaveKey);

    /// <summary>Sauvegarde l'étage actuel dans PlayerPrefs.</summary>
    // Écrit l'étage courant et force la persistance disque
    public static void SaveGame()
    {
        PlayerPrefs.SetInt(SaveKey, GameProgress.Instance.CurrentFloor);
        PlayerPrefs.Save();
    }

    /// <summary>Restaure la progression depuis la sauvegarde.</summary>
    // Relit l'étage sauvegardé et avance GameProgress en conséquence
    public static void LoadGame()
    {
        int savedFloor = PlayerPrefs.GetInt(SaveKey, 1);

        // Avance le singleton jusqu'à l'étage sauvegardé
        for (int i = 1; i < savedFloor; i++)
        {
            GameProgress.Instance.AdvanceFloor();
        }
    }

    /// <summary>Supprime la sauvegarde existante.</summary>
    // Efface la clé et force la persistance disque
    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }
}
