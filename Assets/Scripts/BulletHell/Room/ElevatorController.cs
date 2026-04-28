using UnityEngine;
using UnityEngine.Events;

public class ElevatorController : MonoBehaviour
{
    [SerializeField] private RoomController[] _requiredRooms;
    [SerializeField] private bool _requireAllRooms = true;
    [SerializeField] private UnityEvent _onElevatorUnlocked;
    [SerializeField] private UnityEvent _onFloorTransitionRequested;

    private bool _isUnlocked = false;
    private bool _playerInRange = false;

    private void OnEnable() // S'abonne à OnRoomCleared de chaque salle
    {
        if (_requiredRooms == null) return;
        foreach (RoomController room in _requiredRooms) // Parcourt les salles requises
        {
            if (room == null) continue;
            room.OnRoomCleared.AddListener(OnRoomCleared);
        }
    }

    private void OnDisable() // Désabonne les listeners des salles
    {
        if (_requiredRooms == null) return;
        foreach (RoomController room in _requiredRooms) // Retire chaque listener
        {
            if (room == null) continue;
            room.OnRoomCleared.RemoveListener(OnRoomCleared);
        }
    }

    private void OnRoomCleared() // Vérifie la condition de déverrouillage
    {
        if (_isUnlocked) return;
        if (CheckUnlockCondition())
            UnlockElevator();
    }

    private bool CheckUnlockCondition() // Évalue si toutes les salles requises sont libérées
    {
        if (_requiredRooms == null || _requiredRooms.Length == 0) return false;

        if (_requireAllRooms) // Toutes les salles doivent être libérées
        {
            foreach (RoomController room in _requiredRooms)
            {
                if (room == null) continue;
                if (!room.IsCleared()) return false; // Une salle non libérée bloque
            }
            return true;
        }

        foreach (RoomController room in _requiredRooms) // Au moins une salle suffit
        {
            if (room == null) continue;
            if (room.IsCleared()) return true;
        }
        return false;
    }

    private void UnlockElevator() // Marque déverrouillé et notifie les abonnés
    {
        _isUnlocked = true;
        _onElevatorUnlocked?.Invoke();
    }

    public void OnPlayerEnterRange() => _playerInRange = true; // Signale entrée dans la zone

    public void OnPlayerExitRange() => _playerInRange = false; // Signale sortie de la zone

    public void ConfirmInteraction() // Déclenche la transition si conditions remplies
    {
        if (!_isUnlocked || !_playerInRange) return;
        _onFloorTransitionRequested?.Invoke();
    }

    public bool IsUnlocked() => _isUnlocked; // Expose l'état de déverrouillage
}
