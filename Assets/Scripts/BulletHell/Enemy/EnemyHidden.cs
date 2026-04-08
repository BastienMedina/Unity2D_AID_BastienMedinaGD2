using UnityEngine;

// Reste caché, se révèle à proximité du joueur, puis charge
public class EnemyHidden : EnemyBase, IEnemyInjectable
{
    // Énumère les trois états du cycle de vie de l'ennemi caché
    private enum State { Hidden, Revealing, Active }

    // Rayon de détection du joueur pour déclencher la révélation
    [SerializeField] private float _triggerRadius = 2f;

    // Durée de la phase de révélation avant activation du chargeur
    [SerializeField] private float _revealDuration = 0.8f;

    // Intervalle de vérification de la distance joueur en secondes
    [SerializeField] private float _proximityCheckInterval = 0.2f;

    // Objet visible représentant la cachette (meuble, bureau, etc.)
    [SerializeField] private GameObject _hiddenVisual;

    // Objet visible représentant l'ennemi révélé (caché au départ)
    [SerializeField] private GameObject _enemyVisual;

    // Composant EnemyCharger activé une fois l'ennemi révélé
    [SerializeField] private EnemyCharger _charger;

    // Référence au Transform du joueur pour le calcul de distance
    [SerializeField] private Transform _playerTransform;

    // Gestionnaire de butin à déclencher à la mort de l'ennemi
    [SerializeField] private LootSystem _lootSystem;

    // État courant dans le cycle Hidden → Revealing → Active
    private State _currentState = State.Hidden;

    /// <summary>Injecte playerTransform et lootSystem après un Instantiate runtime.</summary>
    public void InjectDependencies(UnityEngine.Transform playerTransform, LivesManager livesManager, LootSystem lootSystem)
    {
        _playerTransform = playerTransform;
        _lootSystem      = lootSystem;

        // Transmet également les dépendances au chargeur interne
        if (_charger != null)
        {
            _charger.InjectDependencies(playerTransform, livesManager, lootSystem);
        }
    }

    // Initialise les visuels et démarre la vérification de proximité
    protected override void Awake()
    {
        // Appelle l'initialisation de la santé définie dans EnemyBase
        base.Awake();

        // Affiche la cachette et masque l'ennemi au démarrage
        if (_hiddenVisual != null) _hiddenVisual.SetActive(true);
        if (_enemyVisual  != null) _enemyVisual.SetActive(false);

        // Désactive le chargeur jusqu'à la fin de la révélation
        if (_charger != null) _charger.enabled = false;

        // Lance la vérification périodique de la proximité joueur
        InvokeRepeating(nameof(CheckPlayerProximity), 0f, _proximityCheckInterval);
    }

    // Vérifie si le joueur est entré dans le rayon de déclenchement
    private void CheckPlayerProximity()
    {
        // Ignore la vérification si l'état n'est pas Hidden
        if (_currentState != State.Hidden)
        {
            return;
        }

        // Ignore si la référence au joueur n'est pas assignée
        if (_playerTransform == null)
        {
            return;
        }

        // Passe en révélation si le joueur est dans le rayon
        if (Vector3.Distance(transform.position, _playerTransform.position) <= _triggerRadius)
        {
            // Démarre la séquence de révélation de l'ennemi caché
            StartReveal();
        }
    }

    // Lance le swap visuel et programme l'activation du chargeur
    private void StartReveal()
    {
        // Passe dans l'état de révélation pour bloquer les checks
        _currentState = State.Revealing;

        // Masque la cachette et affiche le sprite de l'ennemi
        _hiddenVisual.SetActive(false);
        _enemyVisual.SetActive(true);

        // Active le chargeur après la durée de révélation configurée
        Invoke(nameof(ActivateCharger), _revealDuration);
    }

    // Active le composant EnemyCharger et passe en état actif
    private void ActivateCharger()
    {
        // Permet au chargeur d'exécuter sa logique de patrouille
        _charger.enabled = true;

        // Marque l'ennemi comme pleinement actif après révélation
        _currentState = State.Active;
    }

    // Masque l'ennemi et déclenche le butin à la mort
    protected override void HandleDeath()
    {
        // Annule les appels en attente pour éviter des états parasites
        CancelInvoke();

        // Désactive le chargeur pour stopper tout mouvement résiduel
        if (_charger != null) _charger.enabled = false;

        // Cache le visuel de l'ennemi mort
        if (_enemyVisual != null) _enemyVisual.SetActive(false);

        // Demande au système de butin de générer le loot à la position
        _lootSystem?.SpawnLoot(transform.position);
    }
}
