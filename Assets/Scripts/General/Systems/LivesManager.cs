using UnityEngine;
using UnityEngine.Events;

// Gère le nombre de vies restantes du joueur.
public class LivesManager : MonoBehaviour
{
    // Instance statique accessible depuis n'importe quel script
    public static LivesManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Paramètres configurables
    // -------------------------------------------------------------------------

    // Nombre maximum de vies au démarrage de la partie.
    [SerializeField] private int _maxLives = 3;

    // -------------------------------------------------------------------------
    // Événements publics
    // -------------------------------------------------------------------------

    // Déclenché à chaque changement avec la nouvelle valeur.
    public UnityEvent<int> OnLivesChanged = new UnityEvent<int>();

    // Déclenché quand toutes les vies atteignent zéro.
    public UnityEvent OnDeath = new UnityEvent();

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    // Nombre de vies restantes au moment courant.
    private int _currentLives;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Initialise les vies et notifie les abonnés au démarrage.
    private void Awake()
    {
        // Enregistre l'instance unique ou détruit le doublon
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Assigne cette instance comme référence singleton globale
        Instance = this;

        // Restaure les vies depuis GameProgress si elles ont été sauvegardées
        if (GameProgress.Instance != null && GameProgress.Instance.HasPersistedLives)
        {
            _currentLives = GameProgress.Instance.PopLives();
        }
        else
        {
            // Première initialisation ou nouvelle partie : vies au maximum
            _currentLives = _maxLives;
        }

        // Notifie immédiatement les abonnés avec la valeur initiale.
        OnLivesChanged.Invoke(_currentLives);
    }

    // Libère le slot singleton à la destruction pour permettre la réinitialisation à la prochaine scène.
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // -------------------------------------------------------------------------
    // API publique
    // -------------------------------------------------------------------------

    /// <summary>Retourne le nombre de vies restantes actuellement.</summary>
    public int GetCurrentLives()
    {
        // Retourne directement la valeur de l'état interne.
        return _currentLives;
    }

    /// <summary>Décrémente les vies et notifie les abonnés.</summary>
    public void LoseLife()
    {
        // Empêche de descendre en dessous de zéro vie.
        if (_currentLives <= 0)
            return;

        // Décrémente les vies et notifie tous les abonnés.
        _currentLives--;
        OnLivesChanged.Invoke(_currentLives);
    }

    /// <summary>Réinitialise les vies à la valeur maximale.</summary>
    public void ResetLives()
    {
        // Restaure les vies au maximum et notifie les abonnés.
        _currentLives = _maxLives;
        OnLivesChanged.Invoke(_currentLives);
    }

    /// <summary>Restaure un nombre de vies et notifie les abonnés.</summary>
    public void Heal(int amount)
    {
        // Ignore le soin si les vies sont déjà au maximum configuré.
        if (_currentLives >= _maxLives)
            return;

        // Ajoute le montant de soin sans dépasser le maximum de vies.
        _currentLives = Mathf.Min(_currentLives + amount, _maxLives);

        // Notifie les abonnés du nouveau total de vies après soin.
        OnLivesChanged.Invoke(_currentLives);
    }

    /// <summary>Réduit les vies d'un point et notifie les abonnés.</summary>
    public void TakeDamage()
    {
        // Ignore si les vies sont déjà à zéro.
        if (_currentLives <= 0)
            return;

        // Décrémente le compteur de vies actuel.
        _currentLives--;

        // Notifie les abonnés du nouveau total de vies.
        OnLivesChanged.Invoke(_currentLives);

        // Déclenche la mort si toutes les vies sont perdues.
        if (_currentLives <= 0)
            OnDeath.Invoke();
    }

    // Met à jour le maximum de vies du joueur
    public void SetMaxHealth(int newMax)
    {
        // Met à jour le maximum configurable
        _maxLives = newMax;

        // Notifie les abonnés du changement
        OnLivesChanged.Invoke(_currentLives);
    }
}
