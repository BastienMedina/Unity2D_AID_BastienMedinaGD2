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

    // Son joué quand le joueur perd une vie
    [SerializeField] private AudioClip _losLifeClip;

    // Son joué quand le joueur est soigné
    [SerializeField] private AudioClip _healClip;

    // Son joué quand le joueur meurt (game over)
    [SerializeField] private AudioClip _deathClip;

    // -------------------------------------------------------------------------
    // Événements publics
    // -------------------------------------------------------------------------

    // Déclenché à chaque changement avec la nouvelle valeur.
    public UnityEvent<int> OnLivesChanged = new UnityEvent<int>();

    // Déclenché quand le maximum de vies change, avec la nouvelle valeur max.
    public UnityEvent<int> OnMaxHealthChanged = new UnityEvent<int>();

    // Déclenché quand toutes les vies atteignent zéro.
    public UnityEvent OnDeath = new UnityEvent();

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    // Durée en secondes d'invincibilité après un dégât — bloque tout TakeDamage entrant
    [SerializeField] private float _invincibilityDuration = 0.8f;

    // Timer décroissant de l'invincibilité post-dégât
    private float _invincibilityTimer = 0f;

    // Nombre de vies restantes au moment courant.
    private int _currentLives;

    // Décrémente le timer d'invincibilité chaque frame.
    private void Update()
    {
        if (_invincibilityTimer > 0f)
            _invincibilityTimer -= Time.deltaTime;
    }

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

        // Restaure le PV max persisté en priorité (potion de vitalité inter-étages)
        if (GameProgress.Instance != null && GameProgress.Instance.HasPersistedMaxLives)
            _maxLives = GameProgress.Instance.PopMaxLives();

        // Restaure les vies courantes depuis GameProgress si disponibles
        if (GameProgress.Instance != null && GameProgress.Instance.HasPersistedLives)
        {
            _currentLives = GameProgress.Instance.PopLives();
            // Clamp au cas où les vies seraient supérieures au max restauré
            _currentLives = Mathf.Min(_currentLives, _maxLives);
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
        AudioManager.Instance?.PlaySFX(_losLifeClip);
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

        AudioManager.Instance?.PlaySFX(_healClip);

        // Notifie les abonnés du nouveau total de vies après soin.
        OnLivesChanged.Invoke(_currentLives);
    }

    /// <summary>Réduit les vies d'un point et notifie les abonnés.</summary>
    public void TakeDamage()
    {
        // Ignore si les vies sont déjà à zéro.
        if (_currentLives <= 0)
            return;

        // Ignore si le joueur est encore invincible suite au dernier dégât.
        if (_invincibilityTimer > 0f)
            return;

        // Décrémente le compteur de vies actuel.
        _currentLives--;

        // Démarre la fenêtre d'invincibilité post-dégât.
        _invincibilityTimer = _invincibilityDuration;

        // Notifie les abonnés du nouveau total de vies.
        OnLivesChanged.Invoke(_currentLives);

        // Déclenche la mort si toutes les vies sont perdues.
        if (_currentLives <= 0)
        {
            AudioManager.Instance?.PlaySFX(_deathClip);
            OnDeath.Invoke();
        }
        else
        {
            AudioManager.Instance?.PlaySFX(_losLifeClip);
        }
    }

    /// <summary>Met à jour le maximum de vies du joueur et clamp les vies courantes.</summary>
    public void SetMaxHealth(int newMax)
    {
        // Met à jour le maximum configurable
        _maxLives = newMax;

        // Empêche les vies courantes de dépasser le nouveau maximum
        _currentLives = Mathf.Min(_currentLives, _maxLives);

        // Notifie les abonnés du changement de maximum (reconstruit les coeurs UI)
        OnMaxHealthChanged.Invoke(_maxLives);

        // Notifie les abonnés du changement de vies courantes
        OnLivesChanged.Invoke(_currentLives);
    }

    /// <summary>Retourne le maximum de vies actuel.</summary>
    public int GetMaxLives() => _maxLives;
}
