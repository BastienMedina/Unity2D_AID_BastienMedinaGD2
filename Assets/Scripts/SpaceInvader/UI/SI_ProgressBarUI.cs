using UnityEngine;
using UnityEngine.UI;

public class SI_ProgressBarUI : MonoBehaviour
{
    [SerializeField] private SI_ProgressionGauge _progressionGauge;
    [SerializeField] private Image _fillImage;
    [SerializeField] private float _refreshInterval = 0.5f;

    private float _timer;

    private void Awake() // Valide les références et initialise le fill à zéro
    {
        if (_progressionGauge == null) Debug.LogError("[SI_ProgressBarUI] _progressionGauge non assigné.");
        if (_fillImage == null) { Debug.LogError("[SI_ProgressBarUI] _fillImage non assignée."); return; }
        _fillImage.fillAmount = 0f;
    }

    private void Update() // Rafraîchit le fillAmount à intervalle régulier
    {
        if (_progressionGauge == null || _fillImage == null) return;

        _timer += Time.deltaTime;
        if (_timer >= _refreshInterval) // Applique la progression à l'intervalle configuré
        {
            _timer = 0f;
            _fillImage.fillAmount = _progressionGauge.GetProgress();
        }
    }

    public void OnProgressUpdated(float progress) // Applique directement la valeur normalisée reçue
    {
        if (_fillImage == null) return;
        _fillImage.fillAmount = progress;
    }
}
