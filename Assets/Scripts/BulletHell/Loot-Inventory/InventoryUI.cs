using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private int _columnCount = 3;
    [SerializeField] private Vector2 _slotSize = new Vector2(100f, 100f);
    [SerializeField] private Vector2 _slotSpacing = new Vector2(10f, 10f);
    [SerializeField] private Color _emptySlotColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color _filledSlotColor = new Color(0.6f, 0.5f, 0.2f, 1f);
    [SerializeField] private Color _selectedSlotColor = new Color(0.9f, 0.8f, 0.1f, 1f);
    [SerializeField] private Color _noIconColor = new Color(0.2f, 0.7f, 0.4f, 1f);

    [SerializeField] private InventoryManager _inventoryManager;
    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private TextMeshProUGUI _itemDescText;
    [SerializeField] private Button _useButton;
    [SerializeField] private Button _toggleButton;

    private Image[] _slotBackgrounds;
    private Image[] _slotIcons;
    private TextMeshProUGUI[] _slotQuantityBadges;
    private int _selectedSlotIndex = -1;
    private const int NoSelection = -1;

    private void Awake() // Initialise la grille et masque le panneau au démarrage
    {
        if (_inventoryManager == null)
            Debug.LogWarning("[InventoryUI] _inventoryManager non assigné.", this);

        if (_inventoryPanel == null)
            Debug.LogWarning("[InventoryUI] _inventoryPanel non assigné.", this);

        if (_slotPrefab == null)
            Debug.LogWarning("[InventoryUI] _slotPrefab non assigné.", this);

        BuildSlotGrid();

        if (_useButton != null)
            _useButton.onClick.AddListener(OnUseButtonClicked);

        if (_toggleButton == null)
        {
            GameObject toggleGO = GameObject.Find("Button_Inventory");
            if (toggleGO != null)
                _toggleButton = toggleGO.GetComponent<Button>();
            else
                Debug.LogWarning("[InventoryUI] Button_Inventory introuvable dans la scène.", this);
        }

        if (_toggleButton != null)
            _toggleButton.onClick.AddListener(ToggleInventory);

        if (_inventoryPanel != null)
            _inventoryPanel.SetActive(false);

        ClearDescriptionPanel();
    }

    private void OnEnable() // Abonne les événements de l'InventoryManager
    {
        if (_inventoryManager == null) return;
        _inventoryManager.OnItemAdded.AddListener(RefreshSlot);
        _inventoryManager.OnItemConsumed.AddListener(OnItemConsumedHandler);
    }

    private void OnDisable() // Désabonne les événements de l'InventoryManager
    {
        if (_inventoryManager == null) return;
        _inventoryManager.OnItemAdded.RemoveListener(RefreshSlot);
        _inventoryManager.OnItemConsumed.RemoveListener(OnItemConsumedHandler);
    }

    private void OnDestroy() // Retire les listeners boutons à la destruction
    {
        if (_useButton != null)
            _useButton.onClick.RemoveListener(OnUseButtonClicked);

        if (_toggleButton != null)
            _toggleButton.onClick.RemoveListener(ToggleInventory);
    }

    public void ToggleInventory() // Affiche ou masque le panneau d'inventaire
    {
        if (_inventoryPanel == null) return;

        if (_inventoryPanel.activeSelf)
        {
            Deselect();
            _inventoryPanel.SetActive(false);
            Time.timeScale = 1f; // Restaure la vitesse normale à la fermeture
            return;
        }

        _inventoryPanel.SetActive(true);
        RefreshAllSlots();
        Time.timeScale = 0.05f; // Pause légère pendant la consultation
    }

    private void OnSlotClicked(int slotIndex) // Traite le clic selon l'état du slot
    {
        InventoryItem item = _inventoryManager != null ? _inventoryManager.GetItem(slotIndex) : null;

        if (item == null) { Deselect(); return; }

        if (_selectedSlotIndex == slotIndex) // Consomme si déjà sélectionné
        {
            _inventoryManager.ConsumeItem(slotIndex);
            return;
        }

        SelectSlot(slotIndex, item);
    }

    private void SelectSlot(int slotIndex, InventoryItem item) // Sélectionne et affiche les détails du slot
    {
        if (_selectedSlotIndex != NoSelection)
            ApplySlotColor(_selectedSlotIndex, GetSlotColor(_selectedSlotIndex));

        _selectedSlotIndex = slotIndex;
        ApplySlotColor(slotIndex, _selectedSlotColor);
        UpdateDescriptionPanel(item);
    }

    private void Deselect() // Réinitialise la sélection et vide la description
    {
        if (_selectedSlotIndex != NoSelection)
            ApplySlotColor(_selectedSlotIndex, GetSlotColor(_selectedSlotIndex));

        _selectedSlotIndex = NoSelection;
        ClearDescriptionPanel();
    }

    public void RefreshSlot(int slotIndex) // Rafraîchit l'affichage du slot à l'index donné
    {
        if (!IsValidSlotIndex(slotIndex)) return;

        Debug.Log($"[UI] Slot {slotIndex} rafraîchi");

        InventoryItem item = _inventoryManager != null ? _inventoryManager.GetItem(slotIndex) : null;

        if (item == null)
        {
            ApplySlotColor(slotIndex, _emptySlotColor);

            if (_slotIcons[slotIndex] != null)
                _slotIcons[slotIndex].enabled = false;

            SetQuantityBadge(slotIndex, 0);
            return;
        }

        bool isSelected = slotIndex == _selectedSlotIndex;
        ApplySlotColor(slotIndex, isSelected ? _selectedSlotColor : _filledSlotColor);

        if (_slotIcons[slotIndex] != null)
        {
            _slotIcons[slotIndex].enabled = true;

            if (item.Icon != null) // Affiche l'icône réelle si disponible
            {
                _slotIcons[slotIndex].sprite = item.Icon;
                _slotIcons[slotIndex].color  = Color.white;
            }
            else // Couleur de substitution si pas d'icône
            {
                _slotIcons[slotIndex].sprite = null;
                _slotIcons[slotIndex].color  = _noIconColor;
            }
        }

        SetQuantityBadge(slotIndex, item.Quantity);
    }

    private void RefreshAllSlots() // Rafraîchit tous les slots de la grille
    {
        if (_slotBackgrounds == null) return;

        for (int i = 0; i < _slotBackgrounds.Length; i++) // Parcourt chaque slot
            RefreshSlot(i);
    }

    private void UpdateDescriptionPanel(InventoryItem item) // Met à jour les textes du panneau de description
    {
        if (_itemNameText != null)
            _itemNameText.text = item.Name;

        if (_itemDescText != null)
            _itemDescText.text = item.Description;
    }

    private void ClearDescriptionPanel() // Vide tous les textes du panneau de description
    {
        if (_itemNameText != null) _itemNameText.text = string.Empty;
        if (_itemDescText  != null) _itemDescText.text  = string.Empty;
    }

    private void OnItemConsumedHandler(int slotIndex) // Désélectionne et rafraîchit le slot consommé
    {
        if (_selectedSlotIndex == slotIndex) Deselect();
        RefreshSlot(slotIndex);
    }

    private void OnUseButtonClicked() // Consomme l'item du slot sélectionné via le bouton
    {
        if (_selectedSlotIndex == NoSelection || _inventoryManager == null) return;
        _inventoryManager.ConsumeItem(_selectedSlotIndex);
    }

    private void BuildSlotGrid() // Instancie les slots dans le GridLayoutGroup du panneau
    {
        if (_inventoryPanel == null || _slotPrefab == null || _inventoryManager == null) return;

        GridLayoutGroup grid = _inventoryPanel.GetComponentInChildren<GridLayoutGroup>();
        if (grid == null)
        {
            Debug.LogWarning("[InventoryUI] Aucun GridLayoutGroup trouvé dans _inventoryPanel.", this);
            return;
        }

        grid.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = _columnCount;
        grid.cellSize        = _slotSize;
        grid.spacing         = _slotSpacing;

        int capacity         = _inventoryManager.GetCapacity();
        _slotBackgrounds     = new Image[capacity];
        _slotIcons           = new Image[capacity];
        _slotQuantityBadges  = new TextMeshProUGUI[capacity];

        for (int i = 0; i < capacity; i++) // Instancie un slot par emplacement
        {
            int capturedIndex = i;
            GameObject slotGO = Instantiate(_slotPrefab, grid.transform);
            slotGO.name = $"Slot_{i}";

            Image[] images = slotGO.GetComponentsInChildren<Image>(true);

            if (images.Length >= 1) _slotBackgrounds[i] = images[0]; // Premier Image = fond
            if (images.Length >= 2) _slotIcons[i]       = images[1]; // Second Image = icône

            if (_slotBackgrounds[i] != null)
                _slotBackgrounds[i].color = _emptySlotColor;

            if (_slotIcons[i] != null)
                _slotIcons[i].enabled = false;

            _slotQuantityBadges[i] = CreateQuantityBadge(slotGO);

            Button slotButton = slotGO.GetComponentInChildren<Button>();
            if (slotButton != null)
                slotButton.onClick.AddListener(() => OnSlotClicked(capturedIndex));
        }
    }

    private TextMeshProUGUI CreateQuantityBadge(GameObject slotGO) // Crée un badge quantité en bas à droite
    {
        GameObject badgeGO = new GameObject("QuantityBadge", typeof(RectTransform));
        badgeGO.transform.SetParent(slotGO.transform, false);

        RectTransform rt     = badgeGO.GetComponent<RectTransform>();
        rt.anchorMin         = new Vector2(1f, 0f);
        rt.anchorMax         = new Vector2(1f, 0f);
        rt.pivot             = new Vector2(1f, 0f);
        rt.anchoredPosition  = new Vector2(-4f, 4f);
        rt.sizeDelta         = new Vector2(36f, 20f);

        TextMeshProUGUI tmp  = badgeGO.AddComponent<TextMeshProUGUI>();
        tmp.fontSize         = 20f;
        tmp.fontStyle        = TMPro.FontStyles.Bold;
        tmp.color            = Color.white;
        tmp.alignment        = TMPro.TextAlignmentOptions.BottomRight;
        tmp.raycastTarget    = false;
        tmp.text             = string.Empty;

        return tmp;
    }

    private void SetQuantityBadge(int slotIndex, int quantity) // Affiche ou masque le badge de quantité
    {
        if (_slotQuantityBadges == null || slotIndex >= _slotQuantityBadges.Length) return;

        TextMeshProUGUI badge = _slotQuantityBadges[slotIndex];
        if (badge == null) return;

        if (quantity <= 1) // Masque si pas de stack
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

    private void ApplySlotColor(int slotIndex, Color color) // Applique une couleur au fond du slot
    {
        if (!IsValidSlotIndex(slotIndex) || _slotBackgrounds[slotIndex] == null) return;
        _slotBackgrounds[slotIndex].color = color;
    }

    private Color GetSlotColor(int slotIndex) // Retourne la couleur selon la présence d'un item
    {
        InventoryItem item = _inventoryManager != null ? _inventoryManager.GetItem(slotIndex) : null;
        return item != null ? _filledSlotColor : _emptySlotColor;
    }

    private bool IsValidSlotIndex(int index) // Vérifie que l'index est dans les bornes
    {
        if (_slotBackgrounds == null) return false;
        return index >= 0 && index < _slotBackgrounds.Length;
    }
}
