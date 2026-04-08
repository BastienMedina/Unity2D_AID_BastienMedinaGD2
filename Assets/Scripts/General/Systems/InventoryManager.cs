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

    // Alloue le tableau de slots selon la capacité configurée, puis restaure l'inventaire persisté.
    private void Awake()
    {
        // Initialise le tableau avec la capacité définie en Inspector.
        _slots = new InventoryItem[_capacity];

        // Journalise un avertissement si LivesManager n'est pas assigné.
        if (_livesManager == null)
        {
            Debug.LogWarning("[InventoryManager] _livesManager non assigné.", this);
        }

        // Restaure l'inventaire sauvegardé par GameProgress si disponible.
        RestoreFromGameProgress();
    }

    // S'abonne à l'événement de déchargement de scène pour sauvegarder avant la transition.
    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    // Se désabonne pour éviter les appels fantômes.
    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    // Sauvegarde l'inventaire courant dans GameProgress juste avant que la scène soit déchargée.
    private void OnSceneUnloaded(Scene scene)
    {
        if (GameProgress.Instance == null)
            return;

        GameProgress.Instance.SaveInventory(_slots);
        Debug.Log("[InventoryManager] Inventaire sauvegardé dans GameProgress avant déchargement de scène.");
    }

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

    /// <summary>Ajoute un item dans le premier emplacement libre.</summary>
    public bool AddItem(InventoryItem item)
    {
        // Refuse l'ajout si l'item transmis est nul.
        if (item == null)
            return false;

        // Parcourt tous les slots pour trouver le premier vide.
        for (int i = 0; i < _slots.Length; i++)
        {
            // Ignore les emplacements déjà occupés par un item.
            if (_slots[i] != null)
                continue;

            // Insère l'item dans le premier emplacement libre trouvé.
            _slots[i] = item;

            // Confirme l'ajout dans la console avec slot et nom.
            Debug.Log($"[INV] Objet ajouté slot={i} name={item.Name}");

            // Notifie les abonnés de l'ajout avec l'index du slot.
            OnItemAdded.Invoke(i);
            return true;
        }

        // Notifie les abonnés que l'inventaire est complètement plein.
        OnInventoryFull.Invoke();
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

    /// <summary>Consomme l'item au slot donné et applique son effet.</summary>
    public bool ConsumeItem(int slotIndex)
    {
        // Refuse si l'index est invalide ou le slot est vide.
        if (!IsValidIndex(slotIndex) || _slots[slotIndex] == null)
            return false;

        // Récupère l'item du slot avant de le supprimer.
        InventoryItem item = _slots[slotIndex];

        // Délègue l'effet à PlayerStatsManager si disponible.
        if (_playerStats != null)
            _playerStats.ApplyItemEffect(item);
        else
            Debug.LogWarning("[INV] _playerStats non assigné");

        // Supprime l'item du slot en le remplaçant par null.
        _slots[slotIndex] = null;

        // Notifie les abonnés de la consommation avec l'index du slot.
        OnItemConsumed.Invoke(slotIndex);
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
    private void ApplyEffect(InventoryItem item)
    {
        // Sélectionne l'action à exécuter selon l'effet legacy de l'item.
        switch (item.Effect)
        {
            // Applique un soin via le LivesManager si disponible.
            case LootEffect.Heal:
                ApplyHeal(item);
                break;

            // Délègue les effets spéciaux aux systèmes externes.
            case LootEffect.PowerUp:
            case LootEffect.NewHeroCartridge:
                OnSpecialItemConsumed.Invoke(item);
                break;

            // N'applique aucun effet pour les items sans effet.
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

        // Appelle le soin sur le LivesManager avec le montant configuré.
        _livesManager.Heal(item.HealAmount);
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
