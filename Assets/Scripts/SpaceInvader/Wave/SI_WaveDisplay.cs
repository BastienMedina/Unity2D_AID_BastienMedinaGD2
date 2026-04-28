using UnityEngine;
using TMPro;

public class SI_WaveDisplay : MonoBehaviour
{
    [SerializeField] private SI_WaveManager _waveManager;
    [SerializeField] private TextMeshProUGUI _waveText;

    private const string WavePrefix = "VAGUE ";
    private int _lastDisplayedWave = -1;

    private void Awake() // Valide les références et affiche la vague 1
    {
        if (_waveManager == null) Debug.LogError("[SI_WaveDisplay] _waveManager non assigné.");
        if (_waveText    == null) { Debug.LogError("[SI_WaveDisplay] _waveText non assigné."); return; }
        SetWaveText(1);
    }

    private void Update() // Détecte les changements de vague via polling
    {
        if (_waveManager == null || _waveText == null) return;

        int currentWave = _waveManager.GetCurrentWave();
        if (currentWave != _lastDisplayedWave) // Met à jour si la vague a changé
        {
            _lastDisplayedWave = currentWave;
            SetWaveText(currentWave);
        }
    }

    public void OnWaveStarted(int waveNumber) => SetWaveText(waveNumber); // Forçage externe de l'affichage

    private void SetWaveText(int waveNumber) // Compose et applique le texte de vague
    {
        if (_waveText == null) return;
        _waveText.text = WavePrefix + Mathf.Max(1, waveNumber);
    }
}
