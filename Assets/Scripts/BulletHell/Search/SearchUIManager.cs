using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchUIManager : MonoBehaviour
{
    private const string SearchButtonGOName = "Button_Search";
    private const string ProgressBarGOName  = "SearchProgressBar_Root";

    [SerializeField] private GameObject _searchButtonGO;
    [SerializeField] private Button _searchButton;
    [SerializeField] private GameObject _searchProgressBarGO;
    [SerializeField] private Image _searchProgressBar;
    [SerializeField] private TextMeshProUGUI _searchLabel;

    private SearchableObject _currentTarget;

    private void Start() // Résout les refs, masque l'UI et câble le bouton
    {
        ResolveReferences();
        HideSearchButton();
        HideProgressBar();

        if (_searchButton != null)
            _searchButton.onClick.AddListener(OnSearchButtonClicked);
        else
            Debug.LogWarning("[SearchUIManager] Button_Search introuvable dans la scène.", this);
    }

    private void OnDestroy() // Désabonne le bouton et la cible courante
    {
        if (_searchButton != null)
            _searchButton.onClick.RemoveListener(OnSearchButtonClicked);

        if (_currentTarget != null)
            UnsubscribeFromTarget(_currentTarget);
    }

    private void ResolveReferences() // Cherche les éléments UI dans la scène si absents
    {
        if (_searchButtonGO == null)
            _searchButtonGO = FindUIGameObject(SearchButtonGOName);

        if (_searchButton == null && _searchButtonGO != null)
            _searchButton = _searchButtonGO.GetComponentInChildren<Button>(true);

        if (_searchButton == null) // Fallback par recherche sur tous les boutons actifs/inactifs
        {
            foreach (Button b in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (b.gameObject.name == SearchButtonGOName) { _searchButton = b; _searchButtonGO = b.gameObject; break; }
            }
        }

        if (_searchProgressBarGO == null)
            _searchProgressBarGO = FindUIGameObject(ProgressBarGOName);

        if (_searchProgressBar == null && _searchProgressBarGO != null) // Cherche SearchBar_Fill en priorité
        {
            Transform fillT = _searchProgressBarGO.transform.Find("SearchBar_Fill");
            if (fillT != null)
                _searchProgressBar = fillT.GetComponent<Image>();

            if (_searchProgressBar == null) // Fallback : premier Image de type Filled
            {
                foreach (Image img in _searchProgressBarGO.GetComponentsInChildren<Image>(true))
                {
                    if (img.type == Image.Type.Filled) { _searchProgressBar = img; break; }
                }
            }
        }

        if (_searchLabel == null && _searchProgressBarGO != null) // Cherche le label dans la barre
        {
            Transform labelT = _searchProgressBarGO.transform.Find("SearchBar_Label");
            if (labelT != null) _searchLabel = labelT.GetComponent<TextMeshProUGUI>();
        }

        if (_searchLabel == null && _searchButtonGO != null) // Fallback : label du bouton
        {
            Transform labelT = _searchButtonGO.transform.Find("Label");
            if (labelT != null) _searchLabel = labelT.GetComponent<TextMeshProUGUI>();
        }
    }

    public void OnPlayerEnterRange(SearchableObject target) // Affiche le bouton pour la cible entrée
    {
        if (target == null) return;

        if (_currentTarget != null && _currentTarget != target) // Désabonne l'ancienne cible
            UnsubscribeFromTarget(_currentTarget);

        _currentTarget = target;
        SubscribeToTarget(_currentTarget);

        if (_searchLabel != null)
            _searchLabel.text = _currentTarget.GetLabel();

        ShowSearchButton();
    }

    public void OnPlayerExitRange(SearchableObject target) // Cache le bouton et réinitialise la barre
    {
        if (_currentTarget != target) return;

        UnsubscribeFromTarget(target);
        _currentTarget = null;
        HideSearchButton();
        HideProgressBar();

        if (_searchProgressBar != null)
            _searchProgressBar.fillAmount = 0f;
    }

    public void OnSearchStarted() // Cache le bouton et affiche la barre de progression
    {
        HideSearchButton();
        if (_searchProgressBarGO != null) _searchProgressBarGO.SetActive(true);
        if (_searchProgressBar  != null)  _searchProgressBar.fillAmount = 0f;
    }

    private void HandleSearchProgressUpdate(float progress) // Met à jour le remplissage de la barre
    {
        if (_searchProgressBar != null)
            _searchProgressBar.fillAmount = progress;
    }

    private void HandleSearchComplete() // Cache la barre à la fin de la fouille
    {
        HideProgressBar();
    }

    private void OnSearchButtonClicked() // Transmet l'ordre de fouille à la cible
    {
        _currentTarget?.StartSearch();
    }

    private void SubscribeToTarget(SearchableObject target) // Abonne les handlers aux événements de la cible
    {
        if (target == null) return;
        target.OnSearchStarted.AddListener(OnSearchStarted);
        target.OnSearchProgressUpdate.AddListener(HandleSearchProgressUpdate);
        target.OnSearchComplete.AddListener(HandleSearchComplete);
    }

    private void UnsubscribeFromTarget(SearchableObject target) // Désabonne les handlers de la cible
    {
        if (target == null) return;
        target.OnSearchStarted.RemoveListener(OnSearchStarted);
        target.OnSearchProgressUpdate.RemoveListener(HandleSearchProgressUpdate);
        target.OnSearchComplete.RemoveListener(HandleSearchComplete);
    }

    private void ShowSearchButton() // Active le bouton de fouille
    {
        if (_searchButtonGO != null) _searchButtonGO.SetActive(true);
    }

    private void HideSearchButton() // Masque le bouton de fouille
    {
        if (_searchButtonGO != null) _searchButtonGO.SetActive(false);
    }

    private void HideProgressBar() // Masque la barre de progression
    {
        if (_searchProgressBarGO != null) _searchProgressBarGO.SetActive(false);
    }

    private static GameObject FindUIGameObject(string goName) // Cherche un GO par nom dans la scène active
    {
        foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            GameObject found = FindInHierarchy(root, goName);
            if (found != null) return found;
        }
        return null;
    }

    private static GameObject FindInHierarchy(GameObject root, string goName) // Parcourt récursivement la hiérarchie
    {
        if (root.name == goName) return root;
        foreach (Transform child in root.transform)
        {
            GameObject found = FindInHierarchy(child.gameObject, goName);
            if (found != null) return found;
        }
        return null;
    }
}
