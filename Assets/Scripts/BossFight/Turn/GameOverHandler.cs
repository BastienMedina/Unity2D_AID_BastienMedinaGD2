using UnityEngine;

// Écoute TurnManager.OnGameOver et déclenche le menu victoire ou défaite approprié.
public class GameOverHandler : MonoBehaviour
{
    // Référence au gestionnaire de tour pour s'abonner à OnGameOver.
    [SerializeField] private TurnManager _turnManager;

    // Contrôleur du menu victoire dans la scène
    [SerializeField] private VictoryMenuController _victoryMenu;

    // Contrôleur du menu game over dans la scène
    [SerializeField] private GameOverMenuController _gameOverMenu;

    // S'abonne à OnGameOver au démarrage
    private void Awake()
    {
        if (_turnManager == null)
        {
            Debug.LogError("[GameOverHandler] _turnManager n'est pas assigné.");
            return;
        }

        _turnManager.OnGameOver.AddListener(HandleGameOver);
    }

    // Se désabonne proprement à la destruction
    private void OnDestroy()
    {
        if (_turnManager != null)
            _turnManager.OnGameOver.RemoveListener(HandleGameOver);
    }

    // -------------------------------------------------------------------------
    // Gestionnaire
    // -------------------------------------------------------------------------

    /// <summary>Affiche le menu victoire ou défaite selon le résultat.</summary>
    private void HandleGameOver(bool isVictory)
    {
        if (isVictory)
            _victoryMenu?.HandleVictory();
        else
            _gameOverMenu?.HandleDeath();
    }
}
