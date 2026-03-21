using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Résout les collisions de position entre joueur et entités.
public class GridCollisionResolver : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Références externes
    // -------------------------------------------------------------------------

    // Référence au gestionnaire de tour pour s'abonner.
    [SerializeField] private TurnManager _turnManager;

    // Référence au gestionnaire de grille pour la position joueur.
    [SerializeField] private GridManager _gridManager;

    // Référence au système laser pour les cellules et l'événement.
    [SerializeField] private LaserSystem _laserSystem;

    // Référence au système de pièces pour la collecte.
    [SerializeField] private CoinSystem _coinSystem;

    // -------------------------------------------------------------------------
    // Événements publics
    // -------------------------------------------------------------------------

    // Déclenché quand toutes les collisions ont été résolues.
    public UnityEvent OnCollisionsResolved = new UnityEvent();

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Valide les références et s'abonne à la fin de tour.
    private void Awake()
    {
        // Lève une exception si TurnManager n'est pas assigné.
        if (_turnManager == null)
        {
            throw new MissingReferenceException(
                $"[GridCollisionResolver] La référence {nameof(_turnManager)} n'est pas assignée."
            );
        }

        // Lève une exception si GridManager n'est pas assigné.
        if (_gridManager == null)
        {
            throw new MissingReferenceException(
                $"[GridCollisionResolver] La référence {nameof(_gridManager)} n'est pas assignée."
            );
        }

        // Lève une exception si LaserSystem n'est pas assigné.
        if (_laserSystem == null)
        {
            throw new MissingReferenceException(
                $"[GridCollisionResolver] La référence {nameof(_laserSystem)} n'est pas assignée."
            );
        }

        // Lève une exception si CoinSystem n'est pas assigné.
        if (_coinSystem == null)
        {
            throw new MissingReferenceException(
                $"[GridCollisionResolver] La référence {nameof(_coinSystem)} n'est pas assignée."
            );
        }

        // Abonne la résolution à l'événement de fin de tour.
        _turnManager.OnTurnProcessed.AddListener(HandleTurnProcessed);
    }

    // Désabonne proprement pour éviter des fuites mémoire.
    private void OnDestroy()
    {
        // Vérifie que la référence TurnManager est encore valide.
        if (_turnManager != null)
        {
            // Supprime l'abonnement à l'événement de fin de tour.
            _turnManager.OnTurnProcessed.RemoveListener(HandleTurnProcessed);
        }
    }

    // -------------------------------------------------------------------------
    // Gestionnaire d'événement privé
    // -------------------------------------------------------------------------

    // Reçoit le signal de fin de tour et résout les collisions.
    private void HandleTurnProcessed()
    {
        // Lance la résolution ordonnée de toutes les collisions.
        ResolveCollisions();
    }

    // -------------------------------------------------------------------------
    // Résolution des collisions
    // -------------------------------------------------------------------------

    // Résout laser puis pièce dans l'ordre strict défini.
    private void ResolveCollisions()
    {
        // Récupère la position actuelle du joueur depuis la grille.
        Vector2Int playerPos = _gridManager.GetPlayerPosition();

        // Résout la collision laser en priorité absolue sur la pièce.
        bool laserHit = CheckLaserCollision(playerPos);

        // Ignore la pièce si le joueur est touché par le laser.
        if (!laserHit)
        {
            // Résout la collision de pièce seulement sans laser actif.
            CheckCoinCollision(playerPos);
        }

        // Notifie les abonnés que toutes les collisions sont résolues.
        OnCollisionsResolved.Invoke();
    }

    // Détecte le chevauchement joueur-laser et déclenche l'événement.
    private bool CheckLaserCollision(Vector2Int playerPos)
    {
        // Récupère les cellules actives du pattern laser courant.
        List<Vector2Int> laserCells = _laserSystem.GetCurrentLaserCells();

        // Vérifie si la position joueur chevauche une cellule laser.
        if (!laserCells.Contains(playerPos))
            return false;

        // Déclenche l'événement de collision laser du système.
        _laserSystem.OnPlayerHitByLaser.Invoke();

        // Signale au code appelant qu'un hit laser s'est produit.
        return true;
    }

    // Détecte le chevauchement joueur-pièce et déclenche la collecte.
    private void CheckCoinCollision(Vector2Int playerPos)
    {
        // Récupère la position de la pièce active depuis CoinSystem.
        Vector2Int? coinPos = _coinSystem.GetCurrentCoinPos();

        // Ignore si aucune pièce n'est actuellement active sur la grille.
        if (coinPos == null)
            return;

        // Ignore si le joueur n'est pas sur la cellule pièce.
        if (playerPos != coinPos.Value)
            return;

        // Déclenche la collecte via la méthode dédiée de CoinSystem.
        _coinSystem.EvaluateCollection(playerPos);
    }
}
