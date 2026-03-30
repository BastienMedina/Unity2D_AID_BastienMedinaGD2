using UnityEngine;
using UnityEngine.Events;

// Classe de base partagée pour tous les virus du jeu
public abstract class VirusBase : MonoBehaviour, IVirusDamageable
{
    // Santé maximale configurable depuis l'inspecteur
    [SerializeField] protected int _maxHealth = 1;

    // Vitesse de descente verticale vers le serveur
    [SerializeField] private float _moveSpeed = 0.5f;

    // Points de score accordés à la destruction de ce virus
    [SerializeField] private int _scoreValue = 10;

    // Événement déclenché à chaque réception de dégâts
    [SerializeField] private UnityEvent<int> _onDamageTaken;

    // Événement déclenché à la mort du virus
    [SerializeField] private UnityEvent _onDeath;

    // Événement C# déclenché à la mort pour les abonnés externes
    public event System.Action OnDeathEvent;

    // Santé courante initialisée à la valeur maximale au démarrage
    private int _currentHealth;

    // Rigidbody2D partagé, non sérialisé pour éviter les conflits héritage
    [System.NonSerialized] protected Rigidbody2D _rigidbody;

    // Initialise la santé courante et récupère le Rigidbody2D
    protected virtual void Awake()
    {
        // Délègue la valeur initiale à la sous-classe si nécessaire
        _currentHealth = GetInitialHealth();

        // Récupère le Rigidbody2D pour le déplacement vertical
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    // Déplace le virus vers le bas chaque frame physique
    private void FixedUpdate()
    {
        // Délègue si la sous-classe gère son propre mouvement
        if (OverridesMovement())
        {
            return;
        }

        // Applique la descente verticale via Rigidbody2D si présent
        if (_rigidbody != null)
        {
            // Déplace via vélocité pour rester cohérent avec la physique
            _rigidbody.linearVelocity = Vector2.down * _moveSpeed;
        }
        else
        {
            // Repli sur transform si le Rigidbody2D est absent
            transform.position += Vector3.down * (_moveSpeed * Time.fixedDeltaTime);
        }
    }

    // Indique si la sous-classe gère son propre déplacement
    protected virtual bool OverridesMovement()
    {
        // Par défaut la base gère la descente verticale
        return false;
    }

    // Retourne la santé initiale, surchargeable par les sous-classes
    protected virtual int GetInitialHealth()
    {
        // Utilise la santé maximale définie dans cette classe de base
        return _maxHealth;
    }

    /// <summary>Applique des dégâts, fire les événements et gère la mort.</summary>
    // Décrémente la santé et déclenche mort si nécessaire
    public void TakeDamage(int amount)
    {
        // Ignore les dégâts si le virus est déjà mort
        if (IsDead())
        {
            return;
        }

        // Réduit la santé courante du montant de dégâts reçu
        _currentHealth -= amount;

        // Notifie les abonnés avec la santé courante après impact
        _onDamageTaken?.Invoke(_currentHealth);

        // Déclenche la séquence de mort si la santé est épuisée
        if (IsDead())
        {
            // Notifie les abonnés que ce virus vient de mourir
            _onDeath?.Invoke();

            // Notifie les abonnés C# externes de la mort de ce virus
            OnDeathEvent?.Invoke();

            // Délègue la logique de mort à la sous-classe concernée
            HandleDeath();
        }
    }

    // Détecte la collision avec le serveur via son tag
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Vérifie le tag du collider touché
        if (other.CompareTag("Server"))
        {
            // Inflige un dégât au serveur via son composant
            other.GetComponent<SI_ServerHealth>()?.TakeDamage(1);

            // Détruit le virus après contact avec le serveur
            Destroy(gameObject);
        }
    }

    /// <summary>Retourne vrai si la santé courante est inférieure ou égale à zéro.</summary>
    // Vérifie si le virus n'a plus de points de vie
    public bool IsDead()
    {
        // Compare la santé courante au seuil de mort
        return _currentHealth <= 0;
    }

    /// <summary>Retourne la valeur de santé actuelle du virus.</summary>
    // Expose la santé courante en lecture seule
    public int GetCurrentHealth()
    {
        // Renvoie la santé courante sans la modifier
        return _currentHealth;
    }

    /// <summary>Retourne les points de score accordés par ce virus.</summary>
    // Expose la valeur de score en lecture seule
    public int GetScoreValue()
    {
        // Renvoie la valeur de score configurée dans l'inspecteur
        return _scoreValue;
    }

    /// <summary>Implémentée par chaque sous-classe pour gérer sa propre mort.</summary>
    // Définit le comportement de mort propre à chaque virus
    protected abstract void HandleDeath();
}
