using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class RunTimerManager : MonoBehaviour
{
    private const string BestTimeKey = "BestRunTime";

    public static RunTimerManager Instance { get; private set; }

    public UnityEvent<float> OnTick              = new UnityEvent<float>();
    public UnityEvent<float, bool> OnRunFinished  = new UnityEvent<float, bool>();

    public float ElapsedTime { get; private set; }
    public float BestTime    => PlayerPrefs.GetFloat(BestTimeKey, 0f);
    public bool  IsRunning   { get; private set; }

    private Coroutine _tickCoroutine;

    private void Awake() // Initialise le singleton persistant
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartRun() // Remet à zéro et démarre le timer
    {
        ElapsedTime = 0f;
        IsRunning   = true;

        if (_tickCoroutine != null) StopCoroutine(_tickCoroutine);
        _tickCoroutine = StartCoroutine(TickCoroutine());
    }

    public void StopRun() // Stoppe le timer et notifie la fin de run
    {
        if (!IsRunning) return;

        IsRunning = false;
        if (_tickCoroutine != null) { StopCoroutine(_tickCoroutine); _tickCoroutine = null; }

        bool isNewBest = RegisterBestTime(ElapsedTime);
        OnRunFinished?.Invoke(ElapsedTime, isNewBest);
    }

    public void ResetTimer() // Stoppe et vide le timer sans déclencher OnRunFinished
    {
        IsRunning   = false;
        ElapsedTime = 0f;
        if (_tickCoroutine != null) { StopCoroutine(_tickCoroutine); _tickCoroutine = null; }
    }

    public void ClearBestTime() // Supprime le record de PlayerPrefs
    {
        PlayerPrefs.DeleteKey(BestTimeKey);
        PlayerPrefs.Save();
    }

    public static string FormatTime(float seconds) // Convertit les secondes en MM:SS.cc
    {
        int min  = (int)(seconds / 60f);
        int sec  = (int)(seconds % 60f);
        int cent = (int)((seconds - Mathf.Floor(seconds)) * 100f);
        return $"{min:D2}:{sec:D2}.{cent:D2}";
    }

    private IEnumerator TickCoroutine() // Incrémente et notifie chaque seconde
    {
        while (IsRunning)
        {
            yield return new WaitForSeconds(1f);
            ElapsedTime += 1f;
            OnTick?.Invoke(ElapsedTime);
        }
    }

    private bool RegisterBestTime(float time) // Enregistre le record si meilleur ou absent
    {
        float current = BestTime;
        if (current <= 0f || time < current) { PlayerPrefs.SetFloat(BestTimeKey, time); PlayerPrefs.Save(); return true; }
        return false;
    }
}
