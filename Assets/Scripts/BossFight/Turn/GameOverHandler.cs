using UnityEngine;

// Écoute TurnManager.OnGameOver et déclenche la transition de scène appropriée.
public class GameOverHandler : MonoBehaviour
{
    // Référence au gestionnaire de tour pour s'abonner à OnGameOver.
    [SerializeField] private TurnManager _turnManager;

    // Nom de la scène chargée en cas de victoire
    [SerializeField] private string _victoryScene = "Scene_MainMenu";

    // Nom de la scène chargée en cas de défaite
    [SerializeField] private string _defeatScene = "Scene_MainMenu";

    // Numéro affiché dans la transition animée en cas de victoire
    [SerializeField] private int _victoryFloorLabel = 5;

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

    // Déclenche la transition animée selon le résultat de la partie
    private void HandleGameOver(bool isVictory)
    {
        if (FloorTransitionAnimator.Instance == null)
        {
            Debug.LogError("[GameOverHandler] FloorTransitionAnimator.Instance est null.");
            return;
        }

        if (isVictory)
        {
            // Victoire : on avance vers la scène suivante avec le label d'étage
            FloorTransitionAnimator.Instance.TransitionToScene(_victoryScene, _victoryFloorLabel);
        }
        else
        {
            // Défaite : retour au menu principal, label neutre
            FloorTransitionAnimator.Instance.TransitionToScene(_defeatScene, 0);
        }
    }
}
