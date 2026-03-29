using UnityEngine;
using UnityEngine.UI;

// Met à jour la barre de progression de survie
public class SI_ProgressBarUI : MonoBehaviour
{
    // Référence à la jauge de progression du mini-jeu
    [SerializeField] private SI_ProgressionGauge _progressionGauge;

    // Image remplie représentant la progression de survie
    [SerializeField] private Image _fillImage;

    // Intervalle de rafraîchissement du fillAmount en secondes
    [SerializeField] private float _refreshInterval = 0.5f;

    // Accumulateur de temps pour le rafraîchissement périodique
    private float _timer;

    // Initialise le fill à zéro au démarrage
    private void Awake()
    {
        // Vérifie que la référence à la jauge est assignée
        if (_progressionGauge == null)
        {
            Debug.LogError("[SI_ProgressBarUI] _progressionGauge non assigné.");
        }

        // Vérifie que la référence à l'image est assignée
        if (_fillImage == null)
        {
            Debug.LogError("[SI_ProgressBarUI] _fillImage non assignée.");
            return;
        }

        // Initialise le fill à zéro avant tout événement reçu
        _fillImage.fillAmount = 0f;
    }

    // Rafraîchit le fillAmount à intervalle régulier via polling
    private void Update()
    {
        // Ignore si l'une des références est absente
        if (_progressionGauge == null || _fillImage == null) return;

        // Accumule le temps depuis le dernier rafraîchissement
        _timer += Time.deltaTime;

        // Applique le rafraîchissement si l'intervalle est atteint
        if (_timer >= _refreshInterval)
        {
            // Remet le timer à zéro pour le prochain cycle
            _timer = 0f;

            // Applique la progression normalisée courante au fillAmount
            _fillImage.fillAmount = _progressionGauge.GetProgress();
        }
    }

    /// <summary>Appelée par UnityEvent Inspector pour forcer un rafraîchissement.</summary>
    // Met à jour le fill avec la valeur normalisée reçue en paramètre
    public void OnProgressUpdated(float progress)
    {
        // Ignore si l'image de remplissage est absente
        if (_fillImage == null) return;

        // Applique la valeur normalisée directement au fillAmount
        _fillImage.fillAmount = progress;
    }
}
