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

    // Points de vie courants décrémentés à chaque dégât reçu
    private int _currentHealth;

    // Initialise la santé courante au maximum au démarrage
    protected virtual void Awake()
    {
        // Copie la valeur maximale comme santé de départ
        _currentHealth = _maxHealth;

        // Vérifie la présence du collider sur l'ennemi
        Collider2D col = GetComponent<Collider2D>();
        Debug.Log($"[COL] EnemyBase Awake — {gameObject.name} | Collider2D={col != null} | isTrigger={col?.isTrigger} | enabled={col?.enabled}");

        // Vérifie que IEnemyDamageable est bien implémenté
        IEnemyDamageable dmg = GetComponent<IEnemyDamageable>();
        Debug.Log($"[COL] EnemyBase Awake — IEnemyDamageable={dmg != null}");
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
        // Confirme que TakeDamage est bien appelé
        Debug.Log($"[COL] TakeDamage CALLED — {gameObject.name} | amount={amount} | currentHP={_currentHealth}");

        // Ignore les dégâts si l'ennemi est déjà mort
        if (IsDead())
        {
            return;
        }

        // Réduit la santé courante du montant de dégâts reçus
        _currentHealth -= amount;

        // Notifie les abonnés que l'ennemi vient de subir des dégâts
        _onDamageTaken?.Invoke();

        // Déclenche la mort si la santé est épuisée
        if (IsDead())
        {
            // Notifie les abonnés que l'ennemi vient de mourir
            OnDeath?.Invoke();

            // Délègue le comportement de mort à la sous-classe
            HandleDeath();
        }
    }

    // Comportement de mort spécifique à chaque type d'ennemi
    protected abstract void HandleDeath();
}
