using UnityEngine;
using UnityEngine.Events;

// Détecte l'entrée du joueur, active les ennemis et suit la salle
public class RoomController : MonoBehaviour
{
    // Énumère les trois états possibles du cycle de vie de la salle
    private enum RoomState { Inactive, Active, Cleared }

    // Tableau de tous les ennemis présents dans cette salle
    [SerializeField] private EnemyBase[] _enemies;

    // Verrouille les sorties dès que le joueur entre dans la salle
    [SerializeField] private bool _lockOnEntry = true;

    // Événement déclenché quand le joueur entre dans la salle
    [SerializeField] public UnityEvent OnRoomEntered;

    // Événement déclenché quand les sorties sont verrouillées
    [SerializeField] public UnityEvent OnRoomLocked;

    // Événement déclenché quand tous les ennemis sont éliminés
    [SerializeField] public UnityEvent OnRoomCleared;

    // État courant de la salle dans son cycle de vie
    private RoomState _currentState = RoomState.Inactive;

    // Nombre d'ennemis encore en vie dans la salle active
    private int _remainingEnemyCount = 0;

    // Désactive tous les ennemis au démarrage de la salle
    private void Awake()
    {
        // Ignore si le tableau d'ennemis est vide ou non assigné
        if (_enemies == null)
        {
            return;
        }

        // Désactive chaque ennemi pour l'état initial Inactive
        foreach (EnemyBase enemy in _enemies)
        {
            // Vérifie que la référence à l'ennemi est valide
            if (enemy == null)
            {
                continue;
            }

            // Cache l'ennemi jusqu'à l'entrée du joueur dans la salle
            enemy.gameObject.SetActive(false);
        }
    }

    // Abonne RoomController aux événements de mort de chaque ennemi
    private void OnEnable()
    {
        // Ignore si le tableau d'ennemis est vide ou non assigné
        if (_enemies == null)
        {
            return;
        }

        // S'abonne à l'événement OnDeath de chaque ennemi configuré
        foreach (EnemyBase enemy in _enemies)
        {
            // Vérifie que la référence à l'ennemi est valide avant abonnement
            if (enemy == null)
            {
                continue;
            }

            // Ajoute le callback de mort à l'événement public de l'ennemi
            enemy.OnDeath.AddListener(OnEnemyDied);
        }
    }

    // Désabonne RoomController des événements de mort à la désactivation
    private void OnDisable()
    {
        // Ignore si le tableau d'ennemis est vide ou non assigné
        if (_enemies == null)
        {
            return;
        }

        // Retire le callback de mort de chaque ennemi configuré
        foreach (EnemyBase enemy in _enemies)
        {
            // Vérifie que la référence à l'ennemi est valide avant retrait
            if (enemy == null)
            {
                continue;
            }

            // Supprime le callback pour éviter les appels fantômes
            enemy.OnDeath.RemoveListener(OnEnemyDied);
        }
    }

    // Détecte l'entrée du joueur et active la salle si inactive
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore les collisions si la salle n'est pas dans l'état Inactive
        if (_currentState != RoomState.Inactive)
        {
            return;
        }

        // Ignore les collisions qui ne proviennent pas du joueur
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // Active la salle suite à l'entrée confirmée du joueur
        ActivateRoom();
    }

    // Active tous les ennemis et déclenche le verrouillage si configuré
    private void ActivateRoom()
    {
        // Passe la salle dans l'état actif pour bloquer les re-triggers
        _currentState = RoomState.Active;

        // Initialise le compteur avec le nombre total d'ennemis valides
        _remainingEnemyCount = 0;

        // Active chaque ennemi et incrémente le compteur d'ennemis vivants
        foreach (EnemyBase enemy in _enemies)
        {
            // Ignore les entrées nulles dans le tableau d'ennemis
            if (enemy == null)
            {
                continue;
            }

            // Rend l'ennemi visible et actif dans la scène
            enemy.gameObject.SetActive(true);

            // Compte cet ennemi comme vivant dans le total de la salle
            _remainingEnemyCount++;
        }

        // Notifie les abonnés que le joueur vient d'entrer dans la salle
        OnRoomEntered?.Invoke();

        // Verrouille les sorties si l'option est activée dans l'inspecteur
        if (_lockOnEntry)
        {
            // Notifie les abonnés pour fermer les sorties de la salle
            OnRoomLocked?.Invoke();
        }

        // Vérifie immédiatement si la salle est vide sans ennemis valides
        CheckRoomCleared();
    }

    // Décrémente le compteur et vérifie si la salle est libérée
    private void OnEnemyDied()
    {
        // Ignore si la salle n'est pas dans l'état actif attendu
        if (_currentState != RoomState.Active)
        {
            return;
        }

        // Réduit le nombre d'ennemis encore en vie dans la salle
        _remainingEnemyCount--;

        // Vérifie si tous les ennemis ont été éliminés après ce décès
        CheckRoomCleared();
    }

    // Passe la salle en état Cleared si tous les ennemis sont morts
    private void CheckRoomCleared()
    {
        // Attend que tous les ennemis soient morts pour libérer la salle
        if (_remainingEnemyCount > 0)
        {
            return;
        }

        // Marque la salle comme libérée pour bloquer tout re-trigger
        _currentState = RoomState.Cleared;

        // Notifie les abonnés que la salle vient d'être entièrement libérée
        OnRoomCleared?.Invoke();

        // Ouvre les sorties si elles avaient été verrouillées à l'entrée
        if (_lockOnEntry)
        {
            // Notifie les abonnés pour ouvrir les sorties de la salle
            OnRoomLocked?.Invoke();
        }
    }

    // Retourne vrai si tous les ennemis de la salle ont été éliminés
    public bool IsCleared()
    {
        // Compare l'état courant à l'état Cleared pour la réponse
        return _currentState == RoomState.Cleared;
    }
}
