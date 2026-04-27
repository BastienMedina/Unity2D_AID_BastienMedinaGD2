using UnityEngine;
using UnityEngine.Events;

// Classe de base abstraite gérant santé, dégâts et mort ennemi
public abstract class EnemyBase : MonoBehaviour, IEnemyDamageable
{
    // Points de vie maximaux configurables depuis l'inspecteur
    [SerializeField] private int _maxHealth = 2;

    // Événement déclenché à chaque fois que l'ennemi subit des dégâts
    [SerializeField] private UnityEvent _onDamageTaken;

    // Événement déclenché une seule fois quand l'ennemi meurt
    [SerializeField] public UnityEvent OnDeath;

    // Son joué à la mort de l'ennemi
    [SerializeField] private AudioClip _deathClip;

    // Points de vie courants décrémentés à chaque dégât reçu
    private int _currentHealth;

    // Référence optionnelle au composant de feedback visuel, accessible aux sous-classes
    protected EnemyFeedback _feedback;

    // Initialise la santé courante au maximum au démarrage
    protected virtual void Awake()
    {
        // Copie la valeur maximale comme santé de départ
        _currentHealth = _maxHealth;

        // Récupère le feedback visuel s'il est présent sur le GameObject
        _feedback = GetComponent<EnemyFeedback>();
    }

    // Retourne la santé actuelle de l'ennemi
    public int GetCurrentHealth()
    {
        // Expose la valeur interne en lecture seule
        return _currentHealth;
    }

    // Retourne vrai si les points de vie sont épuisés
    public bool IsDead()
    {
        // Vérifie que la santé courante est nulle ou négative
        return _currentHealth <= 0;
    }

    // Décrémente la santé et déclenche les événements appropriés
    public void TakeDamage(int amount)
    {
        // Ignore les dégâts si l'ennemi est déjà mort
        if (IsDead())
            return;

        // Réduit la santé courante du montant de dégâts reçus
        _currentHealth -= amount;

        // Joue le feedback visuel de dégât si disponible
        _feedback?.PlayHitFeedback();

        // Notifie les abonnés que l'ennemi vient de subir des dégâts
        _onDamageTaken?.Invoke();

        // Déclenche la mort si la santé est épuisée
        if (IsDead())
        {
            AudioManager.Instance?.PlaySFX(_deathClip);

            // Notifie les abonnés que l'ennemi vient de mourir
            OnDeath?.Invoke();

            // Délègue le comportement de mort à la sous-classe
            HandleDeath();
        }
    }

    // Comportement de mort spécifique à chaque type d'ennemi
    protected abstract void HandleDeath();
}
