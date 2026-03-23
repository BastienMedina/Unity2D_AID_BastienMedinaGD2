using UnityEngine;
using UnityEngine.Events;

// Suit la libération des salles, déverrouille et déclenche la transition
public class ElevatorController : MonoBehaviour
{
    // Salles devant être libérées pour déverrouiller l'ascenseur
    [SerializeField] private RoomController[] _requiredRooms;

    // Si vrai, toutes les salles doivent être libérées pour déverrouiller
    [SerializeField] private bool _requireAllRooms = true;

    // Événement déclenché quand l'ascenseur devient utilisable
    [SerializeField] private UnityEvent _onElevatorUnlocked;

    // Événement déclenché quand le joueur confirme l'utilisation
    [SerializeField] private UnityEvent _onFloorTransitionRequested;

    // Indique si l'ascenseur est actuellement déverrouillé
    private bool _isUnlocked = false;

    // Indique si le joueur est actuellement dans la zone de trigger
    private bool _playerInRange = false;

    // Abonne l'ascenseur aux événements de libération de chaque salle
    private void OnEnable()
    {
        // Ignore si aucune salle n'est assignée dans l'inspecteur
        if (_requiredRooms == null)
        {
            return;
        }

        // S'abonne à l'événement OnRoomCleared de chaque salle requise
        foreach (RoomController room in _requiredRooms)
        {
            // Ignore les entrées nulles dans le tableau de salles
            if (room == null)
            {
                continue;
            }

            // Ajoute le callback de vérification à l'événement de la salle
            room.OnRoomCleared.AddListener(OnRoomCleared);
        }
    }

    // Désabonne l'ascenseur des événements à la désactivation
    private void OnDisable()
    {
        // Ignore si aucune salle n'est assignée dans l'inspecteur
        if (_requiredRooms == null)
        {
            return;
        }

        // Retire le callback de chaque salle pour éviter les fuites
        foreach (RoomController room in _requiredRooms)
        {
            // Ignore les entrées nulles dans le tableau de salles
            if (room == null)
            {
                continue;
            }

            // Supprime le listener pour éviter les appels fantômes
            room.OnRoomCleared.RemoveListener(OnRoomCleared);
        }
    }

    // Vérifie la condition de déverrouillage quand une salle est libérée
    private void OnRoomCleared()
    {
        // Ignore si l'ascenseur est déjà déverrouillé
        if (_isUnlocked)
        {
            return;
        }

        // Vérifie si la condition de déverrouillage est satisfaite
        if (CheckUnlockCondition())
        {
            // Déverrouille l'ascenseur et notifie les abonnés
            UnlockElevator();
        }
    }

    // Retourne vrai si la condition de déverrouillage est remplie
    private bool CheckUnlockCondition()
    {
        // Retourne faux si aucune salle n'est configurée
        if (_requiredRooms == null || _requiredRooms.Length == 0)
        {
            return false;
        }

        // Vérifie que toutes les salles sont libérées si requis
        if (_requireAllRooms)
        {
            // Parcourt chaque salle pour vérifier son état libéré
            foreach (RoomController room in _requiredRooms)
            {
                // Ignore les références nulles dans le tableau
                if (room == null)
                {
                    continue;
                }

                // Retourne faux dès qu'une salle n'est pas libérée
                if (!room.IsCleared())
                {
                    return false;
                }
            }

            // Toutes les salles valides sont libérées, condition remplie
            return true;
        }

        // Vérifie qu'au moins une salle est libérée si mode partiel
        foreach (RoomController room in _requiredRooms)
        {
            // Ignore les références nulles dans le tableau
            if (room == null)
            {
                continue;
            }

            // Retourne vrai dès qu'une salle libérée est trouvée
            if (room.IsCleared())
            {
                return true;
            }
        }

        // Aucune salle libérée trouvée en mode partiel
        return false;
    }

    // Marque l'ascenseur comme déverrouillé et notifie les abonnés
    private void UnlockElevator()
    {
        // Enregistre l'état déverrouillé pour bloquer les re-triggers
        _isUnlocked = true;

        // Notifie les abonnés que l'ascenseur est maintenant accessible
        _onElevatorUnlocked?.Invoke();
    }

    // Enregistre la présence du joueur dans la zone de l'ascenseur
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore les collisions qui ne proviennent pas du joueur
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // Marque le joueur comme présent dans la zone de l'ascenseur
        _playerInRange = true;
    }

    // Retire la présence du joueur quand il quitte la zone
    private void OnTriggerExit2D(Collider2D other)
    {
        // Ignore les sorties qui ne concernent pas le joueur
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // Marque le joueur comme absent de la zone de l'ascenseur
        _playerInRange = false;
    }

    // Appelé par le bouton UI pour confirmer l'utilisation de l'ascenseur
    public void ConfirmInteraction()
    {
        // Ignore si l'ascenseur n'est pas encore déverrouillé
        if (!_isUnlocked)
        {
            return;
        }

        // Ignore si le joueur n'est pas dans la zone de l'ascenseur
        if (!_playerInRange)
        {
            return;
        }

        // Demande au FloorProgressionManager de gérer la transition
        _onFloorTransitionRequested?.Invoke();
    }

    // Retourne vrai si l'ascenseur est déverrouillé et utilisable
    public bool IsUnlocked()
    {
        // Expose l'état interne de déverrouillage en lecture seule
        return _isUnlocked;
    }
}
