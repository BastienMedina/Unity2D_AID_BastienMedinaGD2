using UnityEngine;

public class VirusMini : VirusBase
{
    [SerializeField] private float _scatterSpeed = 2f;
    [SerializeField] private float _diveSpeed = 4f;
    [SerializeField] private float _scatterDuration = 0.6f;
    [SerializeField] private float _serverY = -3.5f;
    [SerializeField] private SI_ServerHealth _serverHealth;

    private float _scatterDirection;
    private float _scatterTimer;
    private VirusMiniState _state = VirusMiniState.Scatter;

    private enum VirusMiniState { Scatter, Dive, Dead }

    protected override void Awake() // Initialise la santé et choisit la direction de dispersion
    {
        base.Awake();

        if (_serverHealth == null)
            Debug.LogError("[VIRUS_MINI] SI_ServerHealth non assigné dans l'inspecteur");

        _scatterDirection = Random.value < 0.5f ? -1f : 1f;
    }

    protected override bool OverridesMovement() => true; // VirusMini gère son propre mouvement

    private void FixedUpdate() // Exécute le déplacement selon la phase courante
    {
        if (_state == VirusMiniState.Dead) return;

        if      (_state == VirusMiniState.Scatter) Scatter(); // Dispersion horizontale
        else if (_state == VirusMiniState.Dive)    Dive();    // Plongée vers le serveur
    }

    private void Scatter() // Déplace le mini latéralement puis passe en Dive
    {
        if (_rigidbody == null) return;

        _rigidbody.MovePosition(_rigidbody.position + new Vector2(_scatterDirection * _scatterSpeed * Time.fixedDeltaTime, 0f));
        _scatterTimer += Time.fixedDeltaTime;

        if (_scatterTimer >= _scatterDuration) // Passe en plongée quand la dispersion est terminée
            _state = VirusMiniState.Dive;
    }

    private void Dive() // Descend vers le serveur et détecte l'impact
    {
        if (_rigidbody == null) return;

        _rigidbody.MovePosition(_rigidbody.position + Vector2.down * (_diveSpeed * Time.fixedDeltaTime));

        if (_rigidbody.position.y <= _serverY) // Déclenche l'impact si le seuil serveur est atteint
            ReachServer();
    }

    private void ReachServer() // Inflige dégâts au serveur et se détruit
    {
        _serverHealth?.TakeDamage(1);
        _state = VirusMiniState.Dead;
        Destroy(gameObject);
    }

    protected override void HandleDeath() // Marque mort et se détruit
    {
        _state = VirusMiniState.Dead;
        Destroy(gameObject);
    }

    public void Inject(SI_ServerHealth serverHealth) // Assigne le serveur si absent de l'inspecteur
    {
        if (_serverHealth == null) _serverHealth = serverHealth;
    }
}
