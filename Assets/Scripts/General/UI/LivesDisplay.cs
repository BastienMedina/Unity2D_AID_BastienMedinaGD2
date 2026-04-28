using UnityEngine;
using TMPro;

public class LivesDisplay : MonoBehaviour
{
    [SerializeField] private LivesManager _livesManager;
    [SerializeField] private TextMeshProUGUI _text;

    private const string LivesPrefix = "VIE ";

    private void Awake() // Résout LivesManager si non assigné en Inspector
    {
        if (_livesManager == null)
            _livesManager = FindFirstObjectByType<LivesManager>();
    }

    private void Start() // Lit les vies initiales après tous les Awake
    {
        if (_livesManager == null) return;
        UpdateText(_livesManager.GetCurrentLives());
    }

    private void OnEnable() // Abonne la mise à jour à OnLivesChanged
    {
        if (_livesManager == null) return;
        _livesManager.OnLivesChanged.AddListener(UpdateText);
    }

    private void OnDisable() // Désabonne pour éviter les fuites mémoire
    {
        if (_livesManager == null) return;
        _livesManager.OnLivesChanged.RemoveListener(UpdateText);
    }

    private void UpdateText(int newValue) // Compose et applique la chaîne affichée
    {
        if (_text == null) return;
        _text.text = LivesPrefix + newValue;
    }
}
