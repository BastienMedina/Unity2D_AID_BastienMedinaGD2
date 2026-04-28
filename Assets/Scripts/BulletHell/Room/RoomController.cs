using UnityEngine;
using UnityEngine.Events;

public class RoomController : MonoBehaviour
{
    private enum RoomState { Inactive, Active, Cleared }

    [SerializeField] private EnemyBase[] _enemies;
    [SerializeField] private bool _lockOnEntry = true;
    [SerializeField] public UnityEvent OnRoomEntered;
    [SerializeField] public UnityEvent OnRoomLocked;
    [SerializeField] public UnityEvent OnRoomCleared;

    private RoomState _currentState = RoomState.Inactive;
    private int _remainingEnemyCount = 0;

    private void Awake() // Désactive tous les ennemis au démarrage
    {
        if (_enemies == null) return;
        foreach (EnemyBase enemy in _enemies) // Parcourt et cache chaque ennemi
        {
            if (enemy == null) continue;
            enemy.gameObject.SetActive(false);
        }
    }

    private void OnEnable() // S'abonne à OnDeath de chaque ennemi
    {
        if (_enemies == null) return;
        foreach (EnemyBase enemy in _enemies)
        {
            if (enemy == null) continue;
            enemy.OnDeath.AddListener(OnEnemyDied);
        }
    }

    private void OnDisable() // Désabonne les listeners de mort
    {
        if (_enemies == null) return;
        foreach (EnemyBase enemy in _enemies)
        {
            if (enemy == null) continue;
            enemy.OnDeath.RemoveListener(OnEnemyDied);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) // Active la salle si le joueur entre
    {
        if (_currentState != RoomState.Inactive || !other.CompareTag("Player")) return;
        ActivateRoom();
    }

    private void ActivateRoom() // Active les ennemis et notifie l'entrée
    {
        _currentState        = RoomState.Active;
        _remainingEnemyCount = 0;

        foreach (EnemyBase enemy in _enemies) // Active et compte chaque ennemi
        {
            if (enemy == null) continue;
            enemy.gameObject.SetActive(true);
            _remainingEnemyCount++;
        }

        OnRoomEntered?.Invoke();
        if (_lockOnEntry) OnRoomLocked?.Invoke(); // Verrouille les sorties si configuré
        CheckRoomCleared();
    }

    private void OnEnemyDied() // Décrémente le compteur et vérifie la libération
    {
        if (_currentState != RoomState.Active) return;
        _remainingEnemyCount--;
        CheckRoomCleared();
    }

    private void CheckRoomCleared() // Libère la salle si tous les ennemis sont morts
    {
        if (_remainingEnemyCount > 0) return;

        _currentState = RoomState.Cleared;
        OnRoomCleared?.Invoke();
        if (_lockOnEntry) OnRoomLocked?.Invoke(); // Déverrouille les sorties
    }

    public bool IsCleared() => _currentState == RoomState.Cleared; // Retourne si la salle est libérée
}
