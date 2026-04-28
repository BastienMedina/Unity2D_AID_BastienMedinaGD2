using UnityEngine;

public class GameOverHandler : MonoBehaviour
{
    [SerializeField] private TurnManager _turnManager;
    [SerializeField] private VictoryMenuController _victoryMenu;
    [SerializeField] private GameOverMenuController _gameOverMenu;

    private void Awake() // S'abonne à OnGameOver du TurnManager
    {
        if (_turnManager == null)
        {
            Debug.LogError("[GameOverHandler] _turnManager n'est pas assigné.");
            return;
        }

        _turnManager.OnGameOver.AddListener(HandleGameOver);
    }

    private void OnDestroy() // Désabonne l'écouteur de fin de partie
    {
        if (_turnManager != null)
            _turnManager.OnGameOver.RemoveListener(HandleGameOver);
    }

    private void HandleGameOver(bool isVictory) // Affiche le menu victoire ou défaite
    {
        if (isVictory)
            _victoryMenu?.HandleVictory();
        else
            _gameOverMenu?.HandleDeath();
    }
}
