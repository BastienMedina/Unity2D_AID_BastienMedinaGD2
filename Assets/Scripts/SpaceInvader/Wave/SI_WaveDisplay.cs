using UnityEngine;
using TMPro;

// Affiche le numéro de vague courant en UI
public class SI_WaveDisplay : MonoBehaviour
{
    // Référence au gestionnaire de vagues Space Invaders
    [SerializeField] private SI_WaveManager _waveManager;

    // Composant texte TMP affichant le numéro de vague
    [SerializeField] private TextMeshProUGUI _waveText;

    // Préfixe affiché devant le numéro de vague courant
    private const string WavePrefix = "VAGUE ";

    // Dernier numéro de vague affiché pour détecter les changements
    private int _lastDisplayedWave = -1;

    // Initialise le texte au démarrage avec la valeur initiale
    private void Awake()
    {
        // Vérifie que la référence au gestionnaire est assignée
        if (_waveManager == null)
        {
            Debug.LogError("[SI_WaveDisplay] _waveManager non assigné.");
        }

        // Vérifie que la référence au texte TMP est assignée
        if (_waveText == null)
        {
            Debug.LogError("[SI_WaveDisplay] _waveText non assigné.");
            return;
        }

        // Initialise l'affichage à la vague 1 avant tout événement
        SetWaveText(1);
    }

    // Détecte les changements de vague via polling chaque frame
    private void Update()
    {
        // Ignore si l'une des références est absente
        if (_waveManager == null || _waveText == null) return;

        // Lit le numéro de vague courant depuis le gestionnaire
        int currentWave = _waveManager.GetCurrentWave();

        // Met à jour le texte uniquement si la vague a changé
        if (currentWave != _lastDisplayedWave)
        {
            // Mémorise la vague affichée pour éviter les re-rendus inutiles
            _lastDisplayedWave = currentWave;
            SetWaveText(currentWave);
        }
    }

    /// <summary>Appelée par UnityEvent Inspector pour forcer l'affichage.</summary>
    // Met à jour le texte avec le numéro de vague reçu en paramètre
    public void OnWaveStarted(int waveNumber)
    {
        // Délègue l'affichage à la méthode privée commune
        SetWaveText(waveNumber);
    }

    // Compose et applique le texte du numéro de vague à l'écran
    private void SetWaveText(int waveNumber)
    {
        // Ignore si le composant texte est absent
        if (_waveText == null) return;

        // Compose et affiche la chaîne avec le numéro de vague
        _waveText.text = WavePrefix + Mathf.Max(1, waveNumber);
    }
}
