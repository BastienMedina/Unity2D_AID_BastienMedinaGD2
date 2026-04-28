using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Affiche la grille d'inventaire et gère la sélection de slots.
public class InventoryUI : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Constantes de mise en page
    // -------------------------------------------------------------------------

    // Nombre de colonnes dans la grille d'inventaire 3x3.
    [SerializeField] private int _columnCount = 3;

    // Taille en pixels de chaque slot de la grille.
    [SerializeField] private Vector2 _slotSize = new Vector2(100f, 100f);

    // Espacement en pixels entre chaque slot de la grille.
    [SerializeField] private Vector2 _slotSpacing = new Vector2(10f, 10f);

    // Couleur de fond d'un slot vide dans la grille.
    [SerializeField] private Color _emptySlotColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    // Couleur de fond d'un slot occupé dans la grille.
    [SerializeField] private Color _filledSlotColor = new Color(0.6f, 0.5f, 0.2f, 1f);

    // Couleur de fond d'un slot actuellement sélectionné.
    [SerializeField] private Color _selectedSlotColor = new Color(0.9f, 0.8f, 0.1f, 1f);

    // Couleur de remplacement quand un item n'a pas d'icône.
    [SerializeField] private Color _noIconColor = new Color(0.2f, 0.7f, 0.4f, 1f);

    // -------------------------------------------------------------------------
    // Références UI configurables
    // -------------------------------------------------------------------------

    // Référence au gestionnaire de données de l'inventaire.
    [SerializeField] private InventoryManager _inventoryManager;

    // Panneau racine de l'inventaire, masqué par défaut.
    [SerializeField] private GameObject _inventoryPanel;

    // Prefab d'un slot UI instancié dans la grille.
    [SerializeField] private GameObject _slotPrefab;

    // Texte affichant le nom de l'item sélectionné.
    [SerializeField] private TextMeshProUGUI _itemNameText;

    // Texte affichant la description de l'item sélectionné.
    [SerializeField] private TextMeshProUGUI _itemDescText;

    // Bouton déclenchant la consommation de l'item sélectionné.
    [SerializeField] private Button _useButton;

    // Bouton ouvrant/fermant le panneau d'inventaire (optionnel, trouvé par nom si null).
    [SerializeField] private Button _toggleButton;

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    // Tableau des images de fond de chaque slot instancié.
    private Image[] _slotBackgrounds;

    // Tableau des images d'icône de chaque slot instancié.
    private Image[] _slotIcons;

    // Tableau des badges de quantité (TextMeshProUGUI) de chaque slot.
    private TextMeshProUGUI[] _slotQuantityBadges;

    // Index du slot actuellement sélectionné, -1 si aucun.
    private int _selectedSlotIndex = -1;

    // Valeur sentinelle indiquant qu'aucun slot n'est sélectionné.
    private const int NoSelection = -1;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Initialise la grille et masque le panneau au démarrage.
    private void Awake()
    {
        // Vérifie que l'InventoryManager est bien assigné en Inspector.
        if (_inventoryManager == null)
        {
            Debug.LogWarning("[InventoryUI] _inventoryManager non assigné.", this);
        }

        // Vérifie que le panneau racine est bien assigné en Inspector.
        if (_inventoryPanel == null)
        {
            Debug.LogWarning("[InventoryUI] _inventoryPanel non assigné.", this);
        }

        // Vérifie que le prefab de slot est bien assigné en Inspector.
        if (_slotPrefab == null)
        {
            Debug.LogWarning("[InventoryUI] _slotPrefab non assigné.", this);
        }

        // Instancie les slots dans la grille au démarrage.
        BuildSlotGrid();

        // Abonne le bouton Use à la méthode de consommation.
        if (_useButton != null)
            _useButton.onClick.AddListener(OnUseButtonClicked);

        // Cherche Button_Inventory par nom si le champ n'est pas assigné en Inspector.
        if (_toggleButton == null)
        {
            GameObject toggleGO = GameObject.Find("Button_Inventory");
            if (toggleGO != null)
                _toggleButton = toggleGO.GetComponent<Button>();
            else
                Debug.LogWarning("[InventoryUI] Button_Inventory introuvable dans la scène.", this);
        }

        // Abonne le bouton d'ouverture de l'inventaire à ToggleInventory.
        if (_toggleButton != null)
            _toggleButton.onClick.AddListener(ToggleInventory);

        // Masque le panneau d'inventaire par défaut au démarrage.
        if (_inventoryPanel != null)
            _inventoryPanel.SetActive(false);

        // Vide le panneau de description au démarrage.
        ClearDescriptionPanel();
    }

    // Abonne les événements de l'InventoryManager à l'activation.
    private void OnEnable()
    {
        // Ignore si l'InventoryManager n'est pas disponible.
        if (_inventoryManager == null)
            return;

        // Abonne le rafraîchissement de slot à l'ajout d'un item.
        _inventoryManager.OnItemAdded.AddListener(RefreshSlot);

        // Abonne le rafraîchissement de slot à la consommation d'item.
        _inventoryManager.OnItemConsumed.AddListener(OnItemConsumedHandler);
    }

    // Désabonne les événements de l'InventoryManager à la désactivation.
    private void OnDisable()
    {
        // Ignore si l'InventoryManager n'est pas disponible.
        if (_inventoryManager == null)
            return;

        // Retire l'abonnement à l'événement d'ajout d'item.
        _inventoryManager.OnItemAdded.RemoveListener(RefreshSlot);

        // Retire l'abonnement à l'événement de consommation d'item.
        _inventoryManager.OnItemConsumed.RemoveListener(OnItemConsumedHandler);
    }

    // Retire le listener du bouton Use quand ce composant est détruit.
    private void OnDestroy()
    {
        // Retire le listener pour éviter des références mortes.
        if (_useButton != null)
            _useButton.onClick.RemoveListener(OnUseButtonClicked);

        // Retire le listener du bouton de toggle pour éviter des références mortes.
        if (_toggleButton != null)
            _toggleButton.onClick.RemoveListener(ToggleInventory);
    }

    // -------------------------------------------------------------------------
    // API publique
    // -------------------------------------------------------------------------

    /// <summary>Affiche ou masque le panneau d'inventaire.
    /// Met le jeu en légère pause (timeScale réduit) pendant la consultation.</summary>
    public void ToggleInventory()
    {
        // Ignore si le panneau racine n'est pas assigné.
        if (_inventoryPanel == null)
            return;

        // Masque le panneau et restaure le timeScale si actuellement visible.
        if (_inventoryPanel.activeSelf)
        {
            // Désélectionne le slot courant avant de masquer.
            Deselect();

            // Masque le panneau d'inventaire.
            _inventoryPanel.SetActive(false);

            // Restaure la vitesse normale du jeu à la fermeture.
            Time.timeScale = 1f;
            return;
        }

        // Affiche le panneau et rafraîchit tous les slots.
        _inventoryPanel.SetActive(true);

        // Rafraîchit l'affichage de chaque slot au moment de l'ouverture.
        RefreshAllSlots();

        // Applique une pause légère pour permettre la consultation sans stopper les animations.
        Time.timeScale = 0.05f;
    }

    // -------------------------------------------------------------------------
    // Gestion des clics sur les slots
    // -------------------------------------------------------------------------

    // Traite le clic sur un slot selon son état courant.
    private void OnSlotClicked(int slotIndex)
    {
        // Récupère l'item présent dans le slot cliqué.
        InventoryItem item = _inventoryManager != null
            ? _inventoryManager.GetItem(slotIndex)
            : null;

        // Désélectionne et vide le panneau si le slot est vide.
        if (item == null)
        {
            // Remet la sélection à son état initial sans cible.
            Deselect();
            return;
        }

        // Consomme l'item si ce slot est déjà le slot sélectionné.
        if (_selectedSlotIndex == slotIndex)
        {
            // Appelle la consommation sur l'InventoryManager.
            _inventoryManager.ConsumeItem(slotIndex);
            return;
        }

        // Sélectionne le slot et affiche les détails de l'item.
        SelectSlot(slotIndex, item);
    }

    // -------------------------------------------------------------------------
    // Sélection et désélection
    // -------------------------------------------------------------------------

    // Sélectionne un slot et met à jour le panneau de description.
    private void SelectSlot(int slotIndex, InventoryItem item)
    {
        // Remet la couleur de l'ancien slot sélectionné si applicable.
        if (_selectedSlotIndex != NoSelection)
            ApplySlotColor(_selectedSlotIndex, GetSlotColor(_selectedSlotIndex));

        // Mémorise le nouvel index sélectionné.
        _selectedSlotIndex = slotIndex;

        // Applique la couleur de sélection au slot cliqué.
        ApplySlotColor(slotIndex, _selectedSlotColor);

        // Met à jour le panneau de description avec l'item sélectionné.
        UpdateDescriptionPanel(item);
    }

    // Réinitialise la sélection et vide le panneau de description.
    private void Deselect()
    {
        // Remet la couleur du slot précédemment sélectionné si actif.
        if (_selectedSlotIndex != NoSelection)
            ApplySlotColor(_selectedSlotIndex, GetSlotColor(_selectedSlotIndex));

        // Efface l'index de sélection courante.
        _selectedSlotIndex = NoSelection;

        // Vide le panneau de description suite à la désélection.
        ClearDescriptionPanel();
    }

    // -------------------------------------------------------------------------
    // Rafraîchissement des slots
    // -------------------------------------------------------------------------

    /// <summary>Rafraîchit l'affichage du slot à l'index donné.</summary>
    public void RefreshSlot(int slotIndex)
    {
        // Ignore si l'index est hors des bornes du tableau de slots.
        if (!IsValidSlotIndex(slotIndex))
            return;

        // Confirme la mise à jour du slot dans la console Unity.
        Debug.Log($"[UI] Slot {slotIndex} rafraîchi");

        // Récupère l'item dans le slot depuis l'InventoryManager.
        InventoryItem item = _inventoryManager != null
            ? _inventoryManager.GetItem(slotIndex)
            : null;

        // Affiche le slot vide si aucun item n'est présent.
        if (item == null)
        {
            // Applique la couleur de slot vide au fond du slot.
            ApplySlotColor(slotIndex, _emptySlotColor);

            // Masque l'image d'icône pour les slots vides.
            if (_slotIcons[slotIndex] != null)
                _slotIcons[slotIndex].enabled = false;

            // Masque le badge de quantité pour les slots vides.
            SetQuantityBadge(slotIndex, 0);
            return;
        }

        // Applique la couleur de fond selon la sélection du slot.
        bool isSelected = slotIndex == _selectedSlotIndex;

        // Utilise la couleur de sélection ou la couleur occupée.
        ApplySlotColor(slotIndex, isSelected ? _selectedSlotColor : _filledSlotColor);

        // Affiche l'icône de l'item si elle est assignée.
        if (_slotIcons[slotIndex] != null)
        {
            // Active l'image d'icône pour ce slot occupé.
            _slotIcons[slotIndex].enabled = true;

            // Affiche l'icône réelle si disponible, sinon couleur de substitution.
            if (item.Icon != null)
            {
                // Assigne le sprite de l'item à l'image du slot.
                _slotIcons[slotIndex].sprite = item.Icon;
                _slotIcons[slotIndex].color = Color.white;
            }
            else
            {
                // Utilise une couleur unie pour les items sans icône.
                _slotIcons[slotIndex].sprite = null;
                _slotIcons[slotIndex].color = _noIconColor;
            }
        }

        // Affiche le badge de quantité uniquement si la pile dépasse 1.
        SetQuantityBadge(slotIndex, item.Quantity);
    }

    // Rafraîchit l'affichage de tous les slots de la grille.
    private void RefreshAllSlots()
    {
        // Ignore si le tableau de slots n'est pas initialisé.
        if (_slotBackgrounds == null)
            return;

        // Parcourt tous les slots et les rafraîchit individuellement.
        for (int i = 0; i < _slotBackgrounds.Length; i++)
        {
            // Appelle le rafraîchissement individuel pour chaque slot.
            RefreshSlot(i);
        }
    }

    // -------------------------------------------------------------------------
    // Panneau de description
    // -------------------------------------------------------------------------

    // Met à jour les textes du panneau de description.
    private void UpdateDescriptionPanel(InventoryItem item)
    {
        // Met à jour le texte du nom si le composant est assigné.
        if (_itemNameText != null)
            _itemNameText.text = item.Name;

        // Met à jour le texte de description si le composant est assigné.
        if (_itemDescText != null)
            _itemDescText.text = item.Description;
    }

    // Vide tous les éléments du panneau de description.
    private void ClearDescriptionPanel()
    {
        // Efface le texte du nom dans le panneau de description.
        if (_itemNameText != null)
            _itemNameText.text = string.Empty;

        // Efface le texte de description dans le panneau.
        if (_itemDescText != null)
            _itemDescText.text = string.Empty;
    }

    // -------------------------------------------------------------------------
    // Handlers d'événements
    // -------------------------------------------------------------------------

    // Rafraîchit le slot et désélectionne si c'était le slot actif.
    private void OnItemConsumedHandler(int slotIndex)
    {
        // Désélectionne si le slot consommé était sélectionné.
        if (_selectedSlotIndex == slotIndex)
            Deselect();

        // Rafraîchit l'affichage du slot consommé.
        RefreshSlot(slotIndex);
    }

    // Consomme l'item du slot sélectionné via le bouton Use.
    private void OnUseButtonClicked()
    {
        // Ignore le clic si aucun slot n'est sélectionné.
        if (_selectedSlotIndex == NoSelection)
            return;

        // Ignore si l'InventoryManager n'est pas disponible.
        if (_inventoryManager == null)
            return;

        // Consomme l'item dans le slot actuellement sélectionné.
        _inventoryManager.ConsumeItem(_selectedSlotIndex);
    }

    // -------------------------------------------------------------------------
    // Construction de la grille
    // -------------------------------------------------------------------------

    // Instancie les slots dans le GridLayoutGroup du panneau.
    private void BuildSlotGrid()
    {
        // Abandonne la construction si une référence requise est nulle.
        if (_inventoryPanel == null || _slotPrefab == null || _inventoryManager == null)
            return;

        // Récupère ou ajoute le GridLayoutGroup sur le panneau.
        GridLayoutGroup grid = _inventoryPanel.GetComponentInChildren<GridLayoutGroup>();

        // Journalise un avertissement si le GridLayoutGroup est absent.
        if (grid == null)
        {
            Debug.LogWarning("[InventoryUI] Aucun GridLayoutGroup trouvé dans _inventoryPanel.", this);
            return;
        }

        // Configure le GridLayoutGroup selon les paramètres sérialisés.
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = _columnCount;
        grid.cellSize = _slotSize;
        grid.spacing = _slotSpacing;

        // Récupère la capacité totale de l'inventaire pour la grille.
        int capacity = _inventoryManager.GetCapacity();

        // Initialise les tableaux de références aux composants des slots.
        _slotBackgrounds    = new Image[capacity];
        _slotIcons          = new Image[capacity];
        _slotQuantityBadges = new TextMeshProUGUI[capacity];

        // Instancie un slot pour chaque emplacement de l'inventaire.
        for (int i = 0; i < capacity; i++)
        {
            // Capture l'index pour l'utiliser dans le lambda du bouton.
            int capturedIndex = i;

            // Instancie le prefab du slot dans le GridLayoutGroup.
            GameObject slotGO = Instantiate(_slotPrefab, grid.transform);
            slotGO.name = $"Slot_{i}";

            // Récupère les composants Image enfants du prefab instancié.
            Image[] images = slotGO.GetComponentsInChildren<Image>(true);

            // Le premier Image est le fond, le second est l'icône.
            if (images.Length >= 1)
                _slotBackgrounds[i] = images[0];

            // Assigne l'image d'icône si un second Image est présent.
            if (images.Length >= 2)
                _slotIcons[i] = images[1];

            // Applique la couleur vide au fond du slot initialisé.
            if (_slotBackgrounds[i] != null)
                _slotBackgrounds[i].color = _emptySlotColor;

            // Masque l'icône du slot vide dès l'initialisation.
            if (_slotIcons[i] != null)
                _slotIcons[i].enabled = false;

            // Crée le badge de quantité en coin inférieur-droit du slot.
            _slotQuantityBadges[i] = CreateQuantityBadge(slotGO);

            // Abonne le bouton du slot à la méthode de clic.
            Button slotButton = slotGO.GetComponentInChildren<Button>();

            // Ajoute le listener si le composant Button est présent.
            if (slotButton != null)
                slotButton.onClick.AddListener(() => OnSlotClicked(capturedIndex));
        }
    }

    // -------------------------------------------------------------------------
    // Badge de quantité
    // -------------------------------------------------------------------------

    // Crée dynamiquement un TextMeshProUGUI en coin inférieur-droit du slot.
    private TextMeshProUGUI CreateQuantityBadge(GameObject slotGO)
    {
        GameObject badgeGO = new GameObject("QuantityBadge", typeof(RectTransform));
        badgeGO.transform.SetParent(slotGO.transform, false);

        // Positionne en bas à droite, en dehors du raycast pour ne pas bloquer les clics.
        RectTransform rt = badgeGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot     = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-4f, 4f);
        rt.sizeDelta = new Vector2(36f, 20f);

        TextMeshProUGUI tmp = badgeGO.AddComponent<TextMeshProUGUI>();
        tmp.fontSize        = 20f;
        tmp.fontStyle       = TMPro.FontStyles.Bold;
        tmp.color           = Color.white;
        tmp.alignment       = TMPro.TextAlignmentOptions.BottomRight;
        tmp.raycastTarget   = false;
        tmp.text            = string.Empty;

        return tmp;
    }

    // Affiche ou masque le badge de quantité selon la valeur reçue.
    private void SetQuantityBadge(int slotIndex, int quantity)
    {
        // Ignore si le tableau n'est pas initialisé ou l'index hors bornes.
        if (_slotQuantityBadges == null || slotIndex >= _slotQuantityBadges.Length)
            return;

        TextMeshProUGUI badge = _slotQuantityBadges[slotIndex];
        if (badge == null) return;

        // Masque le badge si la quantité vaut 1 ou moins (pas de stack).
        if (quantity <= 1)
        {
            badge.text    = string.Empty;
            badge.enabled = false;
        }
        else
        {
            badge.text    = quantity.ToString();
            badge.enabled = true;
        }
    }

    // -------------------------------------------------------------------------
    // Helpers internes
    // -------------------------------------------------------------------------

    // Applique une couleur au fond du slot à l'index donné.
    private void ApplySlotColor(int slotIndex, Color color)
    {
        // Ignore si l'index est invalide ou le fond absent.
        if (!IsValidSlotIndex(slotIndex) || _slotBackgrounds[slotIndex] == null)
            return;

        // Assigne la couleur au composant Image de fond du slot.
        _slotBackgrounds[slotIndex].color = color;
    }

    // Retourne la couleur appropriée selon l'état du slot.
    private Color GetSlotColor(int slotIndex)
    {
        // Récupère l'item présent dans le slot pour déterminer la couleur.
        InventoryItem item = _inventoryManager != null
            ? _inventoryManager.GetItem(slotIndex)
            : null;

        // Retourne la couleur occupée ou vide selon la présence d'un item.
        return item != null ? _filledSlotColor : _emptySlotColor;
    }

    // Vérifie que l'index est dans les bornes du tableau de slots.
    private bool IsValidSlotIndex(int index)
    {
        // Retourne faux si le tableau n'est pas encore initialisé.
        if (_slotBackgrounds == null)
            return false;

        // Retourne vrai si l'index est dans les bornes du tableau.
        return index >= 0 && index < _slotBackgrounds.Length;
    }
}
