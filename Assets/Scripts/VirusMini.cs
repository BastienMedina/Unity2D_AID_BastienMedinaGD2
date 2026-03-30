using UnityEngine;

// Gère uniquement la dispersion et la plongée du mini-virus
public class VirusMini : VirusBase
{
    // Vitesse de déplacement latéral pendant la phase de dispersion
    [SerializeField] private float _scatterSpeed = 2f;

    // Vitesse de descente vers le serveur pendant la phase de plongée
    [SerializeField] private float _diveSpeed = 4f;

    // Durée en secondes de la phase de dispersion horizontale
    [SerializeField] private float _scatterDuration = 0.6f;

    // Position Y du serveur utilisée comme seuil d'impact
    [SerializeField] private float _serverY = -3.5f;

    // Référence au serveur pour infliger des dégâts à l'impact
    [SerializeField] private SI_ServerHealth _serverHealth;

    // Direction horizontale choisie aléatoirement au spawn (-1 ou +1)
    private float _scatterDirection;

    // Temps écoulé depuis le début de la phase de dispersion
    private float _scatterTimer;

    // État courant du mini-virus parmi les trois phases possibles
    private VirusMiniState _state = VirusMiniState.Scatter;

    // Définit les trois phases possibles du cycle de vie du mini-virus
    private enum VirusMiniState
    {
        // Phase initiale de dispersion horizontale après le split
        Scatter,

        // Phase de plongée rectiligne vers le serveur
        Dive,

        // Mini-virus détruit, aucune action autorisée
        Dead
    }

    // Initialise la santé et choisit la direction de dispersion au spawn
    protected override void Awake()
    {
        // Appelle Awake de VirusBase pour initialiser la santé et le Rigidbody2D
        base.Awake();

        // Vérifie que SI_ServerHealth est assigné dans l'inspecteur
        if (_serverHealth == null)
        {
            Debug.LogError("[VIRUS_MINI] SI_ServerHealth non assigné dans l'inspecteur");
        }

        // Tire aléatoirement -1 ou +1 comme direction de dispersion
        _scatterDirection = Random.value < 0.5f ? -1f : 1f;
    }

    // Déclare que VirusMini gère son propre mouvement
    protected override bool OverridesMovement()
    {
        // Empêche VirusBase d'appliquer la descente verticale en parallèle
        return true;
    }

    // Exécute le déplacement selon la phase courante du mini-virus
    private void FixedUpdate()
    {
        // Ignore tout déplacement si le mini-virus est mort
        if (_state == VirusMiniState.Dead)
        {
            return;
        }

        // Applique la dispersion horizontale pendant la première phase
        if (_state == VirusMiniState.Scatter)
        {
            // Exécute le déplacement latéral et avance le timer
            Scatter();
        }
        // Applique la plongée vers le serveur pendant la deuxième phase
        else if (_state == VirusMiniState.Dive)
        {
            // Exécute la descente rectiligne vers la position du serveur
            Dive();
        }
    }

    // Déplace le mini-virus latéralement et passe en Dive si terminé
    private void Scatter()
    {
        // Abandonne si le Rigidbody2D est absent du mini-virus
        if (_rigidbody == null)
        {
            return;
        }

        // Calcule la prochaine position en se déplaçant horizontalement
        Vector2 nextPosition = _rigidbody.position
            + new Vector2(_scatterDirection * _scatterSpeed * Time.fixedDeltaTime, 0f);

        // Déplace le mini-virus vers la position latérale calculée
        _rigidbody.MovePosition(nextPosition);

        // Incrémente le timer de dispersion du temps physique écoulé
        _scatterTimer += Time.fixedDeltaTime;

        // Passe en phase de plongée si la durée de dispersion est atteinte
        if (_scatterTimer >= _scatterDuration)
        {
            // Transition vers la phase de plongée vers le serveur
            _state = VirusMiniState.Dive;
        }
    }

    // Descend le mini-virus vers le serveur et détecte l'impact
    private void Dive()
    {
        // Abandonne si le Rigidbody2D est absent du mini-virus
        if (_rigidbody == null)
        {
            return;
        }

        // Calcule la prochaine position en descendant tout droit
        Vector2 nextPosition = _rigidbody.position
            + Vector2.down * (_diveSpeed * Time.fixedDeltaTime);

        // Déplace le mini-virus vers la position calculée vers le bas
        _rigidbody.MovePosition(nextPosition);

        // Vérifie si le mini-virus a atteint ou dépassé la position du serveur
        if (_rigidbody.position.y <= _serverY)
        {
            // Déclenche l'impact sur le serveur et détruit le mini-virus
            ReachServer();
        }
    }

    // Inflige des dégâts au serveur et détruit le mini-virus à l'impact
    private void ReachServer()
    {
        // Applique un point de dégât au serveur si la référence est valide
        if (_serverHealth != null)
        {
            // Appelle TakeDamage avec un point de dégât sur le serveur
            _serverHealth.TakeDamage(1);
        }

        // Passe en état mort avant de supprimer le GameObject
        _state = VirusMiniState.Dead;

        // Supprime le mini-virus de la scène après l'impact sur le serveur
        Destroy(gameObject);
    }

    // Détruit le GameObject à la mort du mini-virus
    protected override void HandleDeath()
    {
        // Passe en état mort pour bloquer tout déplacement résiduel
        _state = VirusMiniState.Dead;

        // Supprime ce mini-virus de la scène après sa mort au combat
        Destroy(gameObject);
    }

    /// <summary>Injecte SI_ServerHealth depuis l'injecteur de scène.</summary>
    // Assigne la santé du serveur si impossible depuis un prefab
    public void Inject(SI_ServerHealth serverHealth)
    {
        // Assigne le ServerHealth seulement si la référence est absente
        if (_serverHealth == null)
        {
            _serverHealth = serverHealth;
        }
    }
}
