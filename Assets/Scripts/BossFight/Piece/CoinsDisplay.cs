using System.Collections;
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
    // Paramètres de feedback — scale punch
    // -------------------------------------------------------------------------

    // Durée totale du punch de scale à la collecte (secondes).
    [SerializeField] private float _punchDuration = 0.3f;

    // Amplitude du punch (valeur multipliée au scale de base du texte).
    [SerializeField] private float _punchScale = 1.4f;

    // -------------------------------------------------------------------------
    // Constantes
    // -------------------------------------------------------------------------

    // Préfixe affiché devant le compteur de pièces courant (vide, l'icône est une Image séparée).
    private const string CoinsPrefix = "";

    // Séparateur entre le total collecté et la cible de victoire.
    private const string CoinsSeparator = " / ";

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    // Scale de référence du Transform texte au démarrage.
    private Vector3 _textBaseScale;

    // Coroutine de punch en cours.
    private Coroutine _punchCoroutine;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Affiche immédiatement le compte de pièces au réveil.
    private void Awake()
    {
        _textBaseScale = _text != null ? _text.transform.localScale : Vector3.one;

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
        _coinSystem.OnCoinCollected.AddListener(OnCoinCollected);
    }

    // Désabonne le rappel pour éviter les fuites mémoire.
    private void OnDisable()
    {
        // Vérifie que la référence est valide avant le désabonnement.
        if (_coinSystem == null)
            return;

        // Retire la méthode de l'événement lors de la désactivation.
        _coinSystem.OnCoinCollected.RemoveListener(OnCoinCollected);
    }

    // -------------------------------------------------------------------------
    // Méthodes privées
    // -------------------------------------------------------------------------

    // Met à jour le texte et déclenche le punch de scale.
    private void OnCoinCollected(int totalCollected)
    {
        UpdateText(totalCollected);

        if (_text == null)
            return;

        if (_punchCoroutine != null)
            StopCoroutine(_punchCoroutine);

        _punchCoroutine = StartCoroutine(ScalePunchCoroutine());
    }

    // Compose et applique la chaîne affichée à l'écran.
    private void UpdateText(int totalCollected)
    {
        // Ignore la mise à jour si le composant texte est absent.
        if (_text == null)
            return;

        // Compose et applique la chaîne affichée à l'écran.
        _text.text = CoinsPrefix + totalCollected + CoinsSeparator + _victoryTarget;
    }

    // -------------------------------------------------------------------------
    // Feedback — scale punch
    // -------------------------------------------------------------------------

    // Gonfle le Transform du texte puis revient au scale de base.
    private IEnumerator ScalePunchCoroutine()
    {
        float elapsed  = 0f;
        float half     = _punchDuration * 0.5f;
        Vector3 peak   = _textBaseScale * _punchScale;
        Transform t    = _text.transform;

        // Phase montante
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.Lerp(_textBaseScale, peak, Mathf.Clamp01(elapsed / half));
            yield return null;
        }

        // Phase descendante
        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.Lerp(peak, _textBaseScale, Mathf.Clamp01(elapsed / half));
            yield return null;
        }

        t.localScale    = _textBaseScale;
        _punchCoroutine = null;
    }
}
