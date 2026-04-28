using TMPro;
using UnityEngine;

// Affiche le meilleur temps de run dans le menu principal.
// À placer sur un TextMeshProUGUI dans la scène Scene_MainMenu.
[RequireComponent(typeof(TextMeshProUGUI))]
public class BestTimeDisplay : MonoBehaviour
{
    // Texte affiché quand aucun record n'existe encore
    [SerializeField] private string _noRecordText = "MEILLEUR TEMPS : --:--.--";

    // Préfixe du meilleur temps
    [SerializeField] private string _prefix = "MEILLEUR TEMPS : ";

    private TextMeshProUGUI _label;

    private void Awake()
    {
        _label = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        Refresh();
    }

    /// <summary>Met à jour l'affichage avec le meilleur temps enregistré.</summary>
    public void Refresh()
    {
        if (_label == null) return;

        float best = RunTimerManager.Instance != null
            ? RunTimerManager.Instance.BestTime
            : PlayerPrefs.GetFloat("BestRunTime", 0f);

        _label.text = best > 0f
            ? $"{_prefix}{RunTimerManager.FormatTime(best)}"
            : _noRecordText;
    }
}
