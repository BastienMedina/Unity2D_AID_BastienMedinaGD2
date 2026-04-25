using UnityEngine;

// Initialise le visuel sprite du joueur directement sur le GameObject
[DefaultExecutionOrder(-20)]
public class PlayerVisual : MonoBehaviour
{
    // Couleur blanche du joueur configurable dans l'inspecteur
    [SerializeField] private Color _playerColor = new Color(1f, 1f, 1f, 1f);

    // Ordre de tri du sprite du joueur dans le rendu 2D
    [SerializeField] private int _sortingOrder = 3;

    // Échelle appliquée au joueur via son Transform local
    [SerializeField] private float _spriteScale = 0.6f;

    // Visuel géré par PlayerDirectionalSprite et PlayerVisualBuilder — aucune logique ici
}
