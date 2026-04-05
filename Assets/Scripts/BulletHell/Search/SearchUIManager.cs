using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Affiche et masque le bouton de fouille selon les événements.
public class SearchUIManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Références UI configurables
    // -------------------------------------------------------------------------

    // GameObject racine du bouton de fouille à afficher.
    [SerializeField] private GameObject _searchButtonGO;

    // Composant Button déclenchant l'action de fouille.
    [SerializeField] private Button _searchButton;

    // GameObject racine de la barre de progression en bas.
    [SerializeField] private GameObject _searchProgressBarGO;

    // Image remplie représentant la barre de progression.
    [SerializeField] private Image _searchProgressBar;

    // Texte affichant le nom de l'objet fouillable courant.
    [SerializeField] private TextMeshProUGUI _searchLabel;

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    // Objet fouillable actuellement ciblé par le joueur.
    private SearchableObject _currentTarget;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Masque le bouton et abonne le bouton au clic au démarrage.
    private void Awake()
    {
        // Masque le bouton de fouille dès l'initialisation.
        HideSearchButton();

        // Masque la barre de progression dès l'initialisation.
        HideProgressBar();

        // Vérifie que la référence au bouton est bien assignée.
        if (_searchButton == null)
        {
            Debug.LogWarning("[SearchUIManager] _searchButton non assigné.", this);
            return;
        }

        // Abonne la méthode de clic à l'événement OnClick du bouton.
        _searchButton.onClick.AddListener(OnSearchButtonClicked);
    }

    // Désabonne le clic bouton quand ce composant est détruit.
    private void OnDestroy()
    {
        // Retire l'abonnement au clic pour éviter des fuites.
        if (_searchButton != null)
            _searchButton.onClick.RemoveListener(OnSearchButtonClicked);

        // Retire les abonnements encore actifs sur la cible courante.
        if (_currentTarget != null)
            UnsubscribeFromTarget(_currentTarget);
    }

    // -------------------------------------------------------------------------
    // API publique — récepteurs des événements SearchableObject
    // -------------------------------------------------------------------------

    /// <summary>Appelé quand le joueur entre dans le rayon d'un objet fouillable.</summary>
    public void OnPlayerEnterRange(SearchableObject target)
    {
        // Ignore si la cible transmise est nulle.
        if (target == null)
            return;

        // Désabonne l'ancienne cible avant d'en accepter une nouvelle.
        if (_currentTarget != null && _currentTarget != target)
            UnsubscribeFromTarget(_currentTarget);

        // Mémorise la nouvelle cible courante.
        _currentTarget = target;

        // Abonne les handlers UI aux événements de la nouvelle cible.
        SubscribeToTarget(_currentTarget);

        // Met à jour le label avec le nom de l'objet fouillable.
        if (_searchLabel != null)
            _searchLabel.text = _currentTarget.GetLabel();

        // Rend le bouton de fouille visible pour le joueur.
        ShowSearchButton();
    }

    /// <summary>Appelé quand le joueur quitte le rayon d'un objet fouillable.</summary>
    public void OnPlayerExitRange(SearchableObject target)
    {
        // Ignore si la cible transmise ne correspond pas à la cible courante.
        if (_currentTarget != target)
            return;

        // Désabonne les handlers UI des événements de la cible.
        UnsubscribeFromTarget(target);

        // Efface la référence à la cible courante.
        _currentTarget = null;

        // Cache le bouton si le joueur s'éloigne.
        _searchButtonGO.SetActive(false);

        // Cache aussi la barre si fouille annulée.
        _searchProgressBarGO.SetActive(false);

        // Réinitialise la barre à zéro.
        if (_searchProgressBar != null)
            _searchProgressBar.fillAmount = 0f;
    }

    /// <summary>Appelé dès que la fouille commence sur la cible courante.</summary>
    public void OnSearchStarted()
    {
        // Masque le bouton de fouille immédiatement.
        _searchButtonGO.SetActive(false);

        // Affiche la barre de progression en bas.
        _searchProgressBarGO.SetActive(true);

        // Réinitialise la barre à zéro au démarrage.
        _searchProgressBar.fillAmount = 0f;
    }

    // -------------------------------------------------------------------------
    // Handlers des événements internes à la cible courante
    // -------------------------------------------------------------------------

    // Met à jour le remplissage de la barre avec la progression reçue.
    private void HandleSearchProgressUpdate(float progress)
    {
        // Ignore si la barre de progression n'est pas assignée.
        if (_searchProgressBar == null)
            return;

        // Met à jour le remplissage de la barre.
        _searchProgressBar.fillAmount = progress;
    }

    // Masque la barre de progression à la complétion.
    private void HandleSearchComplete()
    {
        // Cache la barre de progression à la fin.
        _searchProgressBarGO.SetActive(false);
    }

    // Transmet l'ordre de fouille à la cible courante si disponible.
    private void OnSearchButtonClicked()
    {
        // Ignore le clic si aucune cible n'est actuellement active.
        if (_currentTarget == null)
            return;

        // Déclenche la fouille sur l'objet fouillable courant.
        _currentTarget.StartSearch();
    }

    // -------------------------------------------------------------------------
    // Abonnements aux événements de la cible
    // -------------------------------------------------------------------------

    // Abonne les handlers aux événements de la cible transmise.
    private void SubscribeToTarget(SearchableObject target)
    {
        // Ignore si la cible transmise est nulle.
        if (target == null)
            return;

        // Abonne le début de fouille au gestionnaire UI.
        target.OnSearchStarted.AddListener(OnSearchStarted);

        // Abonne la mise à jour de progression à son handler.
        target.OnSearchProgressUpdate.AddListener(HandleSearchProgressUpdate);

        // Abonne la complétion de fouille à son handler dédié.
        target.OnSearchComplete.AddListener(HandleSearchComplete);
    }

    // Désabonne les handlers des événements de la cible transmise.
    private void UnsubscribeFromTarget(SearchableObject target)
    {
        // Ignore si la cible transmise est nulle.
        if (target == null)
            return;

        // Retire l'abonnement à l'événement de démarrage.
        target.OnSearchStarted.RemoveListener(OnSearchStarted);

        // Retire l'abonnement à la mise à jour de progression.
        target.OnSearchProgressUpdate.RemoveListener(HandleSearchProgressUpdate);

        // Retire l'abonnement à l'événement de complétion.
        target.OnSearchComplete.RemoveListener(HandleSearchComplete);
    }

    // -------------------------------------------------------------------------
    // Helpers d'affichage
    // -------------------------------------------------------------------------

    // Active le GameObject racine du bouton de fouille.
    private void ShowSearchButton()
    {
        // Ignore si la référence au GameObject n'est pas assignée.
        if (_searchButtonGO == null)
            return;

        // Rend le bouton visible en activant son GameObject.
        _searchButtonGO.SetActive(true);
    }

    // Désactive le GameObject racine du bouton de fouille.
    private void HideSearchButton()
    {
        // Ignore si la référence au GameObject n'est pas assignée.
        if (_searchButtonGO == null)
            return;

        // Masque le bouton en désactivant son GameObject.
        _searchButtonGO.SetActive(false);
    }

    // Désactive le GameObject racine de la barre de progression.
    private void HideProgressBar()
    {
        // Ignore si la référence au GameObject n'est pas assignée.
        if (_searchProgressBarGO == null)
            return;

        // Masque la barre en désactivant son GameObject.
        _searchProgressBarGO.SetActive(false);
    }
}
