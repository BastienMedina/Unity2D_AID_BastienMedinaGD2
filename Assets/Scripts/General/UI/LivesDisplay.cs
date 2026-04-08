using UnityEngine;
using TMPro;

// Affiche uniquement le compteur de vies du joueur.
public class LivesDisplay : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Références configurables
    // -------------------------------------------------------------------------

    // Référence au gestionnaire de vies pour lire les données.
    [SerializeField] private LivesManager _livesManager;

    // Composant texte cible pour l'affichage des vies.
    [SerializeField] private TextMeshProUGUI _text;

    // -------------------------------------------------------------------------
    // Constantes
    // -------------------------------------------------------------------------

    // Préfixe affiché devant le nombre de vies courant.
    private const string LivesPrefix = "VIE ";

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Affiche immédiatement les vies courantes au réveil.
    private void Awake()
    {
        // Cherche le LivesManager dans la scène si non assigné en Inspector.
        if (_livesManager == null)
            _livesManager = FindFirstObjectByType<LivesManager>();

        // Journalise un avertissement si la référence reste absente.
        if (_livesManager == null)
        {
            Debug.LogWarning("[LivesDisplay] LivesManager introuvable dans la scène.", this);
            return;
        }

        // Affiche les vies actuelles dès l'initialisation du composant.
        UpdateText(_livesManager.GetCurrentLives());
    }

    // Abonne le rappel à l'événement de changement de vies.
    private void OnEnable()
    {
        // Vérifie que la référence est valide avant l'abonnement.
        if (_livesManager == null)
            return;

        // Abonne la méthode de mise à jour à l'événement des vies.
        _livesManager.OnLivesChanged.AddListener(UpdateText);
    }

    // Désabonne le rappel pour éviter les fuites mémoire.
    private void OnDisable()
    {
        // Vérifie que la référence est valide avant le désabonnement.
        if (_livesManager == null)
            return;

        // Retire la méthode de l'événement lors de la désactivation.
        _livesManager.OnLivesChanged.RemoveListener(UpdateText);
    }

    // -------------------------------------------------------------------------
    // Méthodes privées
    // -------------------------------------------------------------------------

    // Met à jour le texte avec la nouvelle valeur de vies.
    private void UpdateText(int newValue)
    {
        // Ignore la mise à jour si le composant texte est absent.
        if (_text == null)
            return;

        // Compose et applique la chaîne affichée à l'écran.
        _text.text = LivesPrefix + newValue;
    }
}
