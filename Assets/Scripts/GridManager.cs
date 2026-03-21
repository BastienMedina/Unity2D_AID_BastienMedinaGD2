using UnityEngine;
using UnityEngine.Events;

// Initialise la grille avant le renderer et le turn manager.
[DefaultExecutionOrder(-20)]
// Gère uniquement l'état de la grille et la position du joueur.
public class GridManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Référence externe pour la file d'entrée
    // -------------------------------------------------------------------------

    // Référence au TurnManager pour détecter la fin de tour.
    [SerializeField] private TurnManager _turnManager;
    // -------------------------------------------------------------------------
    // Types imbriqués
    // -------------------------------------------------------------------------

    // Représente les états possibles d'une cellule.
    public enum CellState
    {
        Empty,
        Player,
        Laser,
        Coin
    }

    // Contient la position et l'état d'une cellule.
    public class Cell
    {
        // Position en colonne et ligne de la cellule.
        public Vector2Int Position { get; }

        // État courant de cette cellule.
        public CellState State { get; set; }

        // Initialise la cellule avec sa position de grille.
        public Cell(int col, int row)
        {
            Position = new Vector2Int(col, row);
            State    = CellState.Empty;
        }
    }

    // -------------------------------------------------------------------------
    // Paramètres configurables
    // -------------------------------------------------------------------------

    // Nombre de colonnes de la grille configurable.
    [SerializeField] private int _columns = 5;

    // Nombre de lignes de la grille configurable.
    [SerializeField] private int _rows = 5;

    // Colonne de départ initiale du joueur.
    [SerializeField] private int _startColumn = 2;

    // Ligne de départ initiale du joueur.
    [SerializeField] private int _startRow = 2;

    // -------------------------------------------------------------------------
    // Événements publics
    // -------------------------------------------------------------------------

    // Déclenché après chaque déplacement valide du joueur.
    public UnityEvent<Vector2Int> OnPlayerMoved = new UnityEvent<Vector2Int>();

    // Déclenché quand le joueur atteint un bord.
    public UnityEvent OnMoveFailed = new UnityEvent();

    // -------------------------------------------------------------------------
    // État interne
    // -------------------------------------------------------------------------

    // Tableau 2D représentant toutes les cellules de la grille.
    private Cell[,] _grid;

    // Position actuelle du joueur dans la grille.
    private Vector2Int _playerPosition;

    // Indique qu'un tour est en cours de traitement.
    private bool _isTurnProcessing;

    // Direction mise en file pendant le traitement d'un tour.
    private Vector2Int? _pendingDirection;

    // -------------------------------------------------------------------------
    // Cycle de vie Unity
    // -------------------------------------------------------------------------

    // Initialise uniquement la grille au démarrage du composant.
    private void Awake()
    {
        // Construit la grille et place le joueur en position initiale.
        InitializeGrid();
    }

    // Abonne la file d'entrée quand le composant s'active.
    private void OnEnable()
    {
        // Vérifie la référence avant d'abonner la vidange de file.
        if (_turnManager != null)
            _turnManager.OnTurnProcessed.AddListener(HandleTurnProcessed);
    }

    // Désabonne proprement la file quand le composant se désactive.
    private void OnDisable()
    {
        // Vérifie la référence avant de retirer l'abonnement.
        if (_turnManager != null)
            _turnManager.OnTurnProcessed.RemoveListener(HandleTurnProcessed);
    }

    // -------------------------------------------------------------------------
    // Méthodes publiques appelables depuis l'interface
    // -------------------------------------------------------------------------

    /// <summary>Déplace le joueur d'une cellule vers le haut.</summary>
    public void MoveUp()
    {
        // Confirme que MoveUp est bien appelé par le bouton
        Debug.Log("[DIAG] GridManager.MoveUp() appelé");
        // Déplacement vers la ligne supérieure de la grille.
        TryMove(Vector2Int.up);
    }

    /// <summary>Déplace le joueur d'une cellule vers le bas.</summary>
    public void MoveDown()
    {
        // Confirme que MoveDown est bien appelé par le bouton
        Debug.Log("[DIAG] GridManager.MoveDown() appelé");
        // Déplacement vers la ligne inférieure de la grille.
        TryMove(Vector2Int.down);
    }

    /// <summary>Déplace le joueur d'une cellule vers la gauche.</summary>
    public void MoveLeft()
    {
        // Confirme que MoveLeft est bien appelé par le bouton
        Debug.Log("[DIAG] GridManager.MoveLeft() appelé");
        // Déplacement vers la colonne précédente de la grille.
        TryMove(Vector2Int.left);
    }

    /// <summary>Déplace le joueur d'une cellule vers la droite.</summary>
    public void MoveRight()
    {
        // Confirme que MoveRight est bien appelé par le bouton
        Debug.Log("[DIAG] GridManager.MoveRight() appelé");
        // Déplacement vers la colonne suivante de la grille.
        TryMove(Vector2Int.right);
    }

    // -------------------------------------------------------------------------
    // Accesseurs publics en lecture seule
    // -------------------------------------------------------------------------

    /// <summary>Retourne la cellule à la position donnée, ou null.</summary>
    public Cell GetCell(int col, int row)
    {
        // Retourne null si la position est hors grille.
        if (!IsInsideGrid(col, row))
            return null;

        return _grid[col, row];
    }

    /// <summary>Retourne la position actuelle du joueur.</summary>
    public Vector2Int GetPlayerPosition()
    {
        // Expose la position joueur sans permettre de la modifier.
        return _playerPosition;
    }

    /// <summary>Informe la grille qu'un tour est en cours ou terminé.</summary>
    public void SetTurnProcessing(bool isProcessing)
    {
        // Met à jour le verrou de traitement de tour en cours.
        _isTurnProcessing = isProcessing;
    }

    // -------------------------------------------------------------------------
    // Méthodes privées
    // -------------------------------------------------------------------------

    // Construit chaque cellule et place le joueur au départ.
    private void InitializeGrid()
    {
        _grid = new Cell[_columns, _rows];

        // Crée chaque cellule avec sa position de grille.
        for (int col = 0; col < _columns; col++)
        {
            // Parcourt chaque ligne de la colonne courante.
            for (int row = 0; row < _rows; row++)
            {
                _grid[col, row] = new Cell(col, row);
            }
        }

        // Positionne le joueur sur la cellule de départ.
        _playerPosition = new Vector2Int(
            Mathf.Clamp(_startColumn, 0, _columns - 1),
            Mathf.Clamp(_startRow,    0, _rows    - 1)
        );

        // Marque la cellule de départ comme occupée par le joueur.
        _grid[_playerPosition.x, _playerPosition.y].State = CellState.Player;
    }

    // Valide le déplacement et met à jour l'état de la grille.
    private void TryMove(Vector2Int direction)
    {
        // Affiche la direction reçue et l'état du verrou de tour
        Debug.Log($"[DIAG] TryMove() — direction={direction} isTurnProcessing={_isTurnProcessing}");
        // Met en file si un tour est en cours de traitement.
        if (_isTurnProcessing)
        {
            // Signale que l'entrée est mise en file d'attente
            Debug.Log($"[DIAG] TryMove() — MIS EN FILE direction={direction}");
            // Écrase l'entrée précédente par la dernière reçue.
            _pendingDirection = direction;
            return;
        }

        // Calcule la cellule cible selon la direction donnée.
        Vector2Int target = _playerPosition + direction;

        // Bloque le mouvement si la cible est hors grille.
        if (!IsInsideGrid(target.x, target.y))
        {
            // Signale que le déplacement est bloqué par le bord de grille
            Debug.Log($"[DIAG] TryMove() — BLOQUÉ bord grille target={target}");
            // Notifie les abonnés de l'échec du déplacement.
            OnMoveFailed.Invoke();
            return;
        }

        // Libère la cellule actuellement occupée par le joueur.
        _grid[_playerPosition.x, _playerPosition.y].State = CellState.Empty;

        // Met à jour la position interne du joueur.
        _playerPosition = target;

        // Marque la nouvelle cellule comme occupée par le joueur.
        _grid[_playerPosition.x, _playerPosition.y].State = CellState.Player;

        // Confirme que l'événement OnPlayerMoved va être déclenché
        Debug.Log($"[DIAG] TryMove() — OnPlayerMoved.Invoke() → newPos={_playerPosition} listeners={OnPlayerMoved.GetPersistentEventCount()}");
        // Notifie les abonnés avec la nouvelle position du joueur.
        OnPlayerMoved.Invoke(_playerPosition);
    }

    // Traite l'entrée en file dès que le tour est terminé.
    private void HandleTurnProcessed()
    {
        // Ignore si aucune direction n'est en attente de traitement.
        if (_pendingDirection == null)
            return;

        // Consomme la direction en attente avant de l'exécuter.
        Vector2Int queued = _pendingDirection.Value;
        _pendingDirection = null;

        // Exécute le déplacement mis en file après la fin du tour.
        TryMove(queued);
    }

    // Vérifie qu'une coordonnée appartient à la grille.
    private bool IsInsideGrid(int col, int row)
    {
        // Retourne vrai si col et row sont dans les bornes.
        return col >= 0 && col < _columns
            && row >= 0 && row < _rows;
    }
}
