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

    // Référence au gestionnaire de vies pour déclencher les dégâts.
    [SerializeField] private LivesManager _livesManager;

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

        // Lève une exception si LivesManager n'est pas assigné.
        if (_livesManager == null)
        {
            throw new MissingReferenceException(
                $"[GridCollisionResolver] La référence {nameof(_livesManager)} n'est pas assignée."
            );
        }

        // Abonne la résolution au signal d'évaluation laser du tour.
        _turnManager.OnEvaluateLaser.AddListener(ResolveCollisions);
    }

    // Désabonne proprement pour éviter des fuites mémoire.
    private void OnDestroy()
    {
        // Vérifie que la référence TurnManager est encore valide.
        if (_turnManager != null)
        {
            // Supprime l'abonnement à l'événement d'évaluation laser.
            _turnManager.OnEvaluateLaser.RemoveListener(ResolveCollisions);
        }
    }

    // -------------------------------------------------------------------------
    // Résolution des collisions
    // -------------------------------------------------------------------------

    // Résout laser en priorité, pièce seulement si pas de laser.
    private void ResolveCollisions()
    {
        // Récupère la position actuelle du joueur depuis la grille.
        Vector2Int playerPos = _gridManager.GetPlayerPosition();

        // Récupère les cellules du pattern laser actuel.
        List<Vector2Int> laserCells = _laserSystem.GetCurrentLaserCells();

        // Vérifie chaque cellule laser contre la position joueur.
        foreach (Vector2Int laserCell in laserCells)
        {
            // Compare les coordonnées explicitement sans Equals.
            if (laserCell.x == playerPos.x && laserCell.y == playerPos.y)
            {
                // Inflige des dégâts et stoppe la vérification laser.
                TriggerLaserHit();

                // Notifie les abonnés que la résolution est terminée.
                OnCollisionsResolved.Invoke();
                return;
            }
        }

        // Résout la collision de pièce seulement si pas de laser actif.
        CheckCoinCollision(playerPos);

        // Notifie les abonnés que toutes les collisions sont résolues.
        OnCollisionsResolved.Invoke();
    }

    // Applique les dégâts laser et notifie les systèmes abonnés.
    private void TriggerLaserHit()
    {
        // Vérifie que la référence LivesManager est assignée.
        if (_livesManager == null)
            return;

        // Applique les dégâts au gestionnaire de vies.
        _livesManager.TakeDamage();

        // Notifie les abonnés du hit laser confirmé.
        _laserSystem.OnPlayerHitByLaser.Invoke();
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
