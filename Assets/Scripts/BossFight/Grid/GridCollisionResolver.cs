using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GridCollisionResolver : MonoBehaviour
{
    [SerializeField] private TurnManager _turnManager;
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private LaserSystem _laserSystem;
    [SerializeField] private CoinSystem _coinSystem;
    [SerializeField] private AudioClip _laserHitClip;

    private LivesManager _livesManager;

    public UnityEvent OnCollisionsResolved = new UnityEvent();

    private void Awake() // Valide les références et s'abonne au tour
    {
        if (_turnManager == null)
            throw new MissingReferenceException($"[GridCollisionResolver] {nameof(_turnManager)} non assigné.");

        if (_gridManager == null)
            throw new MissingReferenceException($"[GridCollisionResolver] {nameof(_gridManager)} non assigné.");

        if (_laserSystem == null)
            throw new MissingReferenceException($"[GridCollisionResolver] {nameof(_laserSystem)} non assigné.");

        if (_coinSystem == null)
            throw new MissingReferenceException($"[GridCollisionResolver] {nameof(_coinSystem)} non assigné.");

        _livesManager = LivesManager.Instance;
        if (_livesManager == null)
            _livesManager = FindFirstObjectByType<LivesManager>();

        if (_livesManager == null)
            Debug.LogError("[GridCollisionResolver] LivesManager introuvable.", this);

        _turnManager.OnEvaluateLaser.AddListener(ResolveCollisions);
    }

    private void OnDestroy() // Désabonne l'écouteur de tour
    {
        if (_turnManager != null)
            _turnManager.OnEvaluateLaser.RemoveListener(ResolveCollisions);
    }

    private void ResolveCollisions() // Laser prioritaire puis collision pièce
    {
        Vector2Int playerPos = _gridManager.GetPlayerPosition();
        List<Vector2Int> laserCells = _laserSystem.GetCurrentLaserCells();

        foreach (Vector2Int laserCell in laserCells) // Vérifie chaque cellule laser
        {
            if (laserCell.x == playerPos.x && laserCell.y == playerPos.y)
            {
                TriggerLaserHit();
                OnCollisionsResolved.Invoke();
                return;
            }
        }

        CheckCoinCollision(playerPos); // Vérifie collision pièce si pas laser
        OnCollisionsResolved.Invoke();
    }

    private void TriggerLaserHit() // Inflige dégâts et notifie le laser
    {
        if (_livesManager == null)
            return;

        _livesManager.TakeDamage();
        AudioManager.Instance?.PlaySFX(_laserHitClip);
        _laserSystem.OnPlayerHitByLaser.Invoke();
    }

    private void CheckCoinCollision(Vector2Int playerPos) // Collecte la pièce si chevauchement
    {
        Vector2Int? coinPos = _coinSystem.GetCurrentCoinPos();

        if (coinPos == null)
            return;

        if (playerPos != coinPos.Value)
            return;

        _coinSystem.EvaluateCollection(playerPos);
    }
}
