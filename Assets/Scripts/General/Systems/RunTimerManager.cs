using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Singleton persistant qui mesure le temps d'une run complète (BulletHell → victoire).
// Démarre au chargement du premier étage, s'arrête à la victoire.
// Persiste le meilleur temps via PlayerPrefs.
public class RunTimerManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Clé PlayerPrefs
    // -------------------------------------------------------------------------

    private const string BestTimeKey = "BestRunTime";

    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------

    public static RunTimerManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Événements publics
    // -------------------------------------------------------------------------

    // Déclenché chaque seconde avec le temps écoulé en secondes
    public UnityEvent<float> OnTick = new UnityEvent<float>();

    // Déclenché quand la run se termine avec le temps final et si c'est un record
    public UnityEvent<float, bool> OnRunFinished = new UnityEvent<float, bool>();

    // -------------------------------------------------------------------------
    // Propriétés publiques
    // -------------------------------------------------------------------------

    /// <summary>Temps écoulé depuis le début de la run (secondes).</summary>
    public float ElapsedTime { get; private set; }

    /// <summary>Meilleur temps enregistré (0 si aucun record).</summary>
    public float BestTime => PlayerPrefs.GetFloat(BestTimeKey, 0f);

    /// <summary>Vrai si un timer est en cours.</summary>
    public bool IsRunning { get; private set; }

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    private Coroutine _tickCoroutine;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // -------------------------------------------------------------------------
    // API publique
    // -------------------------------------------------------------------------

    /// <summary>Démarre le timer depuis zéro. Appelé par MainMenuController au lancement d'une run.</summary>
    public void StartRun()
    {
        ElapsedTime = 0f;
        IsRunning   = true;

        if (_tickCoroutine != null)
            StopCoroutine(_tickCoroutine);

        _tickCoroutine = StartCoroutine(TickCoroutine());

        Debug.Log("[RunTimerManager] Timer démarré.");
    }

    /// <summary>Arrête le timer et enregistre le meilleur temps si battu.
    /// Appelé par VictoryMenuController à la victoire.</summary>
    public void StopRun()
    {
        if (!IsRunning) return;

        IsRunning = false;

        if (_tickCoroutine != null)
        {
            StopCoroutine(_tickCoroutine);
            _tickCoroutine = null;
        }

        bool isNewBest = RegisterBestTime(ElapsedTime);

        Debug.Log($"[RunTimerManager] Run terminée — {FormatTime(ElapsedTime)}{(isNewBest ? " ★ Nouveau record !" : "")}");

        OnRunFinished?.Invoke(ElapsedTime, isNewBest);
    }

    /// <summary>Remet le timer à zéro sans le démarrer (utilisé lors du Reset de GameProgress).</summary>
    public void ResetTimer()
    {
        IsRunning   = false;
        ElapsedTime = 0f;

        if (_tickCoroutine != null)
        {
            StopCoroutine(_tickCoroutine);
            _tickCoroutine = null;
        }
    }

    /// <summary>Efface le meilleur temps enregistré.</summary>
    public void ClearBestTime()
    {
        PlayerPrefs.DeleteKey(BestTimeKey);
        PlayerPrefs.Save();
    }

    /// <summary>Formate un temps en secondes vers MM:SS.cc</summary>
    public static string FormatTime(float seconds)
    {
        int min  = (int)(seconds / 60f);
        int sec  = (int)(seconds % 60f);
        int cent = (int)((seconds - Mathf.Floor(seconds)) * 100f);

        return $"{min:D2}:{sec:D2}.{cent:D2}";
    }

    // -------------------------------------------------------------------------
    // Interne
    // -------------------------------------------------------------------------

    // Incrémente ElapsedTime chaque seconde et fire OnTick
    private IEnumerator TickCoroutine()
    {
        while (IsRunning)
        {
            yield return new WaitForSeconds(1f);
            ElapsedTime += 1f;
            OnTick?.Invoke(ElapsedTime);
        }
    }

    // Enregistre le meilleur temps si le nouveau est inférieur (ou si aucun record)
    private bool RegisterBestTime(float time)
    {
        float current = BestTime;

        if (current <= 0f || time < current)
        {
            PlayerPrefs.SetFloat(BestTimeKey, time);
            PlayerPrefs.Save();
            return true;
        }

        return false;
    }
}
