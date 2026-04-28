using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class BestTimeDisplay : MonoBehaviour
{
    [SerializeField] private string _noRecordText = "MEILLEUR TEMPS : --:--.--";
    [SerializeField] private string _prefix       = "MEILLEUR TEMPS : ";

    private TextMeshProUGUI _label;

    private void Awake() // Récupère le composant TMP
    {
        _label = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable() // Rafraîchit l'affichage à chaque activation
    {
        Refresh();
    }

    public void Refresh() // Lit et affiche le meilleur temps enregistré
    {
        if (_label == null) return;

        float best = RunTimerManager.Instance != null
            ? RunTimerManager.Instance.BestTime
            : PlayerPrefs.GetFloat("BestRunTime", 0f);

        _label.text = best > 0f ? $"{_prefix}{RunTimerManager.FormatTime(best)}" : _noRecordText;
    }
}
