using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// Représente un objet stockable dans l'inventaire du joueur.
[System.Serializable]
public class InventoryItem
{
    // Nom court affiché dans l'interface utilisateur.
    public string Name;

    // Description textuelle affichée dans le panneau détail.
    public string Description;

    // Effet legacy utilisé par LootPickup et ApplyEffect.
    public LootEffect Effect;

    // Nombre de vies restaurées lors d'un soin si applicable.
    public int HealAmount;

    // Icône affichée dans la case d'inventaire (peut être nulle).
    public Sprite Icon;

    // Type d'effet étendu provenant de l'ItemData source.
    public ItemEffectType EffectType;

    // Valeur numérique de l'effet (soin, dégâts, vitesse…).
    public float EffectValue;

    // Indique si l'effet est limité dans le temps.
    public bool HasDuration;

    // Durée de l'effet en secondes si HasDuration est vrai.
    public float EffectDuration;

    // Référence à l'ItemData source pour usage futur.
    public ItemData SourceData;

    // Nombre d'exemplaires empilés dans ce slot.
    public int Quantity = 1;
}

// Gère le stockage et les opérations sur l'inventaire joueur.
public class InventoryManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Paramètres configurables
    // -------------------------------------------------------------------------

    // Nombre maximum d'emplacements dans l'inventaire.
    [SerializeField] private int _capacity = 9;

    // Référence au gestionnaire de vies pour les soins.
    [SerializeField] private LivesManager _livesManager;

    // Référence au gestionnaire de stats pour les effets d'items.
    [SerializeField] private PlayerStatsManager _playerStats;

    // Son joué quand un item est ajouté à l'inventaire
    [SerializeField] private AudioClip _itemAddedClip;

    // Son joué quand un item est consommé
    [SerializeField] private AudioClip _itemConsumedClip;

    // Son joué quand l'inventaire est plein
    [SerializeField] private AudioClip _inventoryFullClip;

    // -------------------------------------------------------------------------
    // Événements publics
    // -------------------------------------------------------------------------

    // Déclenché avec l'index du slot après ajout d'un item.
    public UnityEvent<int> OnItemAdded = new UnityEvent<int>();

    // Déclenché avec l'index du slot après consommation d'un item.
    public UnityEvent<int> OnItemConsumed = new UnityEvent<int>();

    // Déclenché quand tous les emplacements sont occupés.
    public UnityEvent OnInventoryFull = new UnityEvent();

    // Déclenché pour les items spéciaux à traiter en externe.
    public UnityEvent<InventoryItem> OnSpecialItemConsumed = new UnityEvent<InventoryItem>();

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    // Tableau des emplacements d'inventaire, null = vide.
    private InventoryItem[] _slots;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Alloue le tableau de slots selon la capacité configurée.
    // La restauration depuis GameProgress est déplacée dans Start()
    // pour garantir que GameProgress.Instance est déjà initialisé.
    private void Awake()
    {
        // Initialise le tableau avec la capacité définie en Inspector.
        _slots = new InventoryItem[_capacity];

        // Résout LivesManager par FindFirstObjectByType si non assigné en Inspector.
        // Cela permet à InventoryManager de fonctionner dans toutes les scènes
        // (BulletHell, GameAndWatch, SpaceInvaders) sans assignation manuelle.
        if (_livesManager == null)
            _livesManager = FindFirstObjectByType<LivesManager>();

        if (_livesManager == null)
            Debug.LogWarning("[InventoryManager] LivesManager introuvable dans la scène — les soins seront inactifs.", this);
    }

    // Restaure l'inventaire persisté ici — après tous les Awake(),
    // donc GameProgress.Instance est garanti non null à ce stade.
    private void Start()
    {
        RestoreFromGameProgress();
    }

    // S'abonne à l'événement de chargement de scène pour détecter le retour au menu.
    private void OnEnable()
    {
        SceneManager.sceneLoaded   += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    // Se désabonne pour éviter les appels fantômes.
    private void OnDisable()
    {
        SceneManager.sceneLoaded   -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    // Réinitialise la progression quand le menu principal est chargé (fin de run).
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (GameProgress.Instance == null) return;

        // Retour au menu = fin de run : réinitialise GameProgress
        if (scene.name == "Scene_MainMenu")
        {
            GameProgress.Instance.Reset();
            Debug.Log("[InventoryManager] Retour au menu — progression réinitialisée.");
        }
    }

    // Callback sceneUnloaded conservé pour d'éventuelles extensions futures.
    private void OnSceneUnloaded(Scene scene) { }

    // Recharge les items persistés depuis GameProgress dans les slots locaux.
    private void RestoreFromGameProgress()
    {
        if (GameProgress.Instance == null || !GameProgress.Instance.HasPersistedInventory())
            return;

        List<InventoryItem> persisted = GameProgress.Instance.PopInventory();

        // Copie chaque item dans le slot correspondant, en respectant la capacité locale.
        for (int i = 0; i < persisted.Count && i < _slots.Length; i++)
        {
            _slots[i] = persisted[i];

            if (_slots[i] != null)
            {
                OnItemAdded.Invoke(i);
                Debug.Log($"[InventoryManager] Slot {i} restauré : {_slots[i].Name}");
            }
        }
    }

    // -------------------------------------------------------------------------
    // API publique
    // -------------------------------------------------------------------------

    /// <summary>Ajoute un item dans l'inventaire.
    /// Si un slot contient déjà le même item (même SourceData ou même Name),
    /// incrémente sa quantité plutôt que d'occuper un nouveau slot.</summary>
    public bool AddItem(InventoryItem item)
    {
        // Refuse l'ajout si l'item transmis est nul.
        if (item == null)
            return false;

        // Cherche d'abord un slot existant contenant le même item.
        for (int i = 0; i < _slots.Length; i++)
        {
            // Ignore les slots vides.
            if (_slots[i] == null)
                continue;

            // Identifie un match par SourceData (priorité) ou par nom (fallback).
            bool sameSource = item.SourceData != null
                && _slots[i].SourceData == item.SourceData;
            bool sameName = !sameSource
                && string.Equals(_slots[i].Name, item.Name, System.StringComparison.Ordinal);

            if (!sameSource && !sameName)
                continue;

            // Incrémente la quantité du slot existant.
            _slots[i].Quantity++;

            Debug.Log($"[INV] Stack slot={i} name={item.Name} qty={_slots[i].Quantity}");

            // Notifie l'UI pour rafraîchir le badge de quantité.
            OnItemAdded.Invoke(i);
            AudioManager.Instance?.PlaySFX(_itemAddedClip);
            return true;
        }

        // Aucun slot existant — cherche le premier slot vide.
        for (int i = 0; i < _slots.Length; i++)
        {
            // Ignore les emplacements déjà occupés par un item.
            if (_slots[i] != null)
                continue;

            // Garantit que la quantité initiale vaut 1.
            item.Quantity = 1;

            // Insère l'item dans le premier emplacement libre trouvé.
            _slots[i] = item;

            Debug.Log($"[INV] Objet ajouté slot={i} name={item.Name}");

            // Notifie les abonnés de l'ajout avec l'index du slot.
            OnItemAdded.Invoke(i);
            AudioManager.Instance?.PlaySFX(_itemAddedClip);
            return true;
        }

        // Notifie les abonnés que l'inventaire est complètement plein.
        OnInventoryFull.Invoke();
        AudioManager.Instance?.PlaySFX(_inventoryFullClip);
        return false;
    }

    /// <summary>Retourne l'item à l'index donné ou null si vide.</summary>
    public InventoryItem GetItem(int slotIndex)
    {
        // Retourne null si l'index est hors des bornes du tableau.
        if (!IsValidIndex(slotIndex))
            return null;

        // Retourne directement la valeur du slot demandé.
        return _slots[slotIndex];
    }

    /// <summary>Consomme une unité de l'item au slot donné et applique son effet.
    /// Libère le slot uniquement quand la quantité atteint zéro.</summary>
    public bool ConsumeItem(int slotIndex)
    {
        // Refuse si l'index est invalide ou le slot est vide.
        if (!IsValidIndex(slotIndex) || _slots[slotIndex] == null)
            return false;

        // Récupère l'item du slot pour appliquer son effet.
        InventoryItem item = _slots[slotIndex];

        // Délègue l'effet à PlayerStatsManager si disponible, sinon applique l'effet legacy.
        if (_playerStats != null)
            _playerStats.ApplyItemEffect(item);
        else
            ApplyEffect(item);

        // Décrémente la quantité avant de décider si le slot se libère.
        item.Quantity--;

        if (item.Quantity <= 0)
        {
            // Quantité épuisée : libère le slot.
            _slots[slotIndex] = null;
            Debug.Log($"[INV] Slot {slotIndex} libéré — {item.Name} épuisé.");
        }
        else
        {
            Debug.Log($"[INV] Consommé slot={slotIndex} name={item.Name} qty restante={item.Quantity}");
        }

        // Notifie les abonnés de la consommation avec l'index du slot.
        OnItemConsumed.Invoke(slotIndex);
        AudioManager.Instance?.PlaySFX(_itemConsumedClip);
        return true;
    }

    /// <summary>Retourne la capacité maximale de l'inventaire.</summary>
    public int GetCapacity()
    {
        // Expose la capacité configurée en lecture seule.
        return _capacity;
    }

    // -------------------------------------------------------------------------
    // Logique d'effet interne
    // -------------------------------------------------------------------------

    // Applique l'effet de l'item selon son type d'effet défini.
    // Priorise EffectType (système étendu) sur Effect (système legacy).
    private void ApplyEffect(InventoryItem item)
    {
        // Tente d'abord d'appliquer via le système étendu (EffectType)
        switch (item.EffectType)
        {
            case ItemEffectType.Heal:
                ApplyHeal(item);
                return;

            case ItemEffectType.MaxHealthUp:
                ApplyMaxHealthUp(item);
                return;

            // Les effets de stat (Speed, Damage, etc.) nécessitent PlayerStatsManager.
            // Sans lui, on logue un avertissement plutôt que d'ignorer silencieusement.
            case ItemEffectType.Speed:
            case ItemEffectType.AttackSpeed:
            case ItemEffectType.Damage:
            case ItemEffectType.Shield:
                Debug.LogWarning($"[InventoryManager] L'effet {item.EffectType} nécessite " +
                    "PlayerStatsManager — assignez-le dans l'Inspector.", this);
                return;

            case ItemEffectType.NewHeroCartridge:
                OnSpecialItemConsumed.Invoke(item);
                return;
        }

        // Fallback sur le système legacy si EffectType n'est pas défini
        switch (item.Effect)
        {
            case LootEffect.Heal:
                ApplyHeal(item);
                break;

            case LootEffect.PowerUp:
            case LootEffect.NewHeroCartridge:
                OnSpecialItemConsumed.Invoke(item);
                break;

            case LootEffect.None:
            default:
                break;
        }
    }

    // Transmet le montant de soin au LivesManager si disponible.
    private void ApplyHeal(InventoryItem item)
    {
        // Journalise un avertissement si le LivesManager est absent.
        if (_livesManager == null)
        {
            Debug.LogWarning("[InventoryManager] Impossible de soigner : _livesManager null.", this);
            return;
        }

        // Utilise EffectValue si défini, sinon HealAmount (legacy)
        int amount = item.EffectValue > 0f ? (int)item.EffectValue : item.HealAmount;

        // Appelle le soin sur le LivesManager avec le montant calculé.
        _livesManager.Heal(amount);
    }

    // Augmente les PV max via le LivesManager si disponible.
    private void ApplyMaxHealthUp(InventoryItem item)
    {
        // Journalise un avertissement si le LivesManager est absent.
        if (_livesManager == null)
        {
            Debug.LogWarning("[InventoryManager] Impossible d'augmenter les PV max : _livesManager null.", this);
            return;
        }

        int amount = (int)item.EffectValue;

        // Incrémente le max de vie dans LivesManager (clamp + événements inclus)
        _livesManager.SetMaxHealth(_livesManager.GetMaxLives() + amount);

        // Soigne le joueur jusqu'au nouveau maximum
        _livesManager.Heal(amount);

        Debug.Log($"[InventoryManager] PV max augmentés via legacy : +{amount}");
    }

    // -------------------------------------------------------------------------
    // Validation interne
    // -------------------------------------------------------------------------

    // Vérifie que l'index est dans les bornes valides du tableau.
    private bool IsValidIndex(int index)
    {
        // Retourne vrai si l'index est compris dans les limites du tableau.
        return index >= 0 && index < _slots.Length;
    }
}
