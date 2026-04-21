using UnityEngine;
using TMPro;

// Affiche uniquement le compteur de pièces collectées.
public class CoinsDisplay : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Références configurables
    // -------------------------------------------------------------------------

    // Référence au système de pièces pour lire les données.
    [SerializeField] private CoinSystem _coinSystem;

    // Composant texte cible pour l'affichage des pièces.
    [SerializeField] private TextMeshProUGUI _text;

    // -------------------------------------------------------------------------
    // Paramètres configurables
    // -------------------------------------------------------------------------

    // Nombre de pièces requises pour déclencher la victoire.
    [SerializeField] private int _victoryTarget = 3;

    // -------------------------------------------------------------------------
    // Constantes
    // -------------------------------------------------------------------------

    // Préfixe affiché devant le compteur de pièces courant (vide, l'icône est une Image séparée).
    private const string CoinsPrefix = "";

    // Séparateur entre le total collecté et la cible de victoire.
    private const string CoinsSeparator = " / ";

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Affiche immédiatement le compte de pièces au réveil.
    private void Awake()
    {
        // Journalise un avertissement si la référence est absente.
        if (_coinSystem == null)
        {
            Debug.LogWarning("[CoinsDisplay] Référence CoinSystem non assignée.", this);
            return;
        }

        // Affiche zéro par défaut car GetCollectedCount n'existe pas.
        UpdateText(0);
    }

    // Abonne le rappel à l'événement de collecte de pièce.
    private void OnEnable()
    {
        // Vérifie que la référence est valide avant l'abonnement.
        if (_coinSystem == null)
            return;

        // Abonne la méthode de mise à jour à l'événement pièces.
        _coinSystem.OnCoinCollected.AddListener(UpdateText);
    }

    // Désabonne le rappel pour éviter les fuites mémoire.
    private void OnDisable()
    {
        // Vérifie que la référence est valide avant le désabonnement.
        if (_coinSystem == null)
            return;

        // Retire la méthode de l'événement lors de la désactivation.
        _coinSystem.OnCoinCollected.RemoveListener(UpdateText);
    }

    // -------------------------------------------------------------------------
    // Méthodes privées
    // -------------------------------------------------------------------------

    // Met à jour le texte avec le total collecté reçu.
    private void UpdateText(int totalCollected)
    {
        // Ignore la mise à jour si le composant texte est absent.
        if (_text == null)
            return;

        // Compose et applique la chaîne affichée à l'écran.
        _text.text = CoinsPrefix + totalCollected + CoinsSeparator + _victoryTarget;
    }
}
