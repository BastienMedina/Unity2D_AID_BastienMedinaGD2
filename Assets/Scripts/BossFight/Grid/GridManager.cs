using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(-20)]
public class GridManager : MonoBehaviour
{
    [SerializeField] private TurnManager _turnManager;

    public enum CellState { Empty, Player, Laser, Coin }

    public class Cell
    {
        public Vector2Int Position { get; }
        public CellState State { get; set; }

        public Cell(int col, int row) // Initialise la cellule à sa position
        {
            Position = new Vector2Int(col, row);
            State    = CellState.Empty;
        }
    }

    [SerializeField] private int _columns = 5;
    [SerializeField] private int _rows = 5;
    [SerializeField] private int _startColumn = 2;
    [SerializeField] private int _startRow = 2;
    [SerializeField] private AudioClip _moveClip;
    [SerializeField] private AudioClip _moveFailClip;

    public UnityEvent<Vector2Int> OnPlayerMoved = new UnityEvent<Vector2Int>();
    public UnityEvent OnMoveFailed = new UnityEvent();

    private Cell[,] _grid;
    private Vector2Int _playerPosition;
    private bool _isTurnProcessing;
    private Vector2Int? _pendingDirection;

    private void Awake() // Initialise la grille et place le joueur
    {
        InitializeGrid();
    }

    private void OnEnable() // S'abonne à la fin de tour
    {
        if (_turnManager != null)
            _turnManager.OnTurnProcessed.AddListener(HandleTurnProcessed);
    }

    private void OnDisable() // Désabonne la file d'entrée
    {
        if (_turnManager != null)
            _turnManager.OnTurnProcessed.RemoveListener(HandleTurnProcessed);
    }

    public void MoveUp() => TryMove(Vector2Int.up); // Déplace le joueur vers le haut
    public void MoveDown() => TryMove(Vector2Int.down); // Déplace le joueur vers le bas
    public void MoveLeft() => TryMove(Vector2Int.left); // Déplace le joueur vers la gauche
    public void MoveRight() => TryMove(Vector2Int.right); // Déplace le joueur vers la droite

    public Cell GetCell(int col, int row) // Retourne la cellule ou null si hors grille
    {
        if (!IsInsideGrid(col, row))
            return null;

        return _grid[col, row];
    }

    public Vector2Int GetPlayerPosition() => _playerPosition; // Expose la position joueur

    public void SetTurnProcessing(bool isProcessing) // Verrouille ou déverrouille les entrées
    {
        _isTurnProcessing = isProcessing;
    }

    private void InitializeGrid() // Crée toutes les cellules et positionne le joueur
    {
        _grid = new Cell[_columns, _rows];

        for (int col = 0; col < _columns; col++) // Parcourt chaque colonne
        {
            for (int row = 0; row < _rows; row++) // Parcourt chaque ligne
            {
                _grid[col, row] = new Cell(col, row);
            }
        }

        _playerPosition = new Vector2Int(
            Mathf.Clamp(_startColumn, 0, _columns - 1),
            Mathf.Clamp(_startRow,    0, _rows    - 1)
        );

        _grid[_playerPosition.x, _playerPosition.y].State = CellState.Player; // Marque la cellule de départ
    }

    private void TryMove(Vector2Int direction) // Valide et applique le déplacement
    {
        if (_isTurnProcessing) // Met en file si tour en cours
        {
            _pendingDirection = direction;
            return;
        }

        Vector2Int target = _playerPosition + direction;

        if (!IsInsideGrid(target.x, target.y)) // Bloque si hors grille
        {
            AudioManager.Instance?.PlaySFX(_moveFailClip);
            OnMoveFailed.Invoke();
            return;
        }

        _grid[_playerPosition.x, _playerPosition.y].State = CellState.Empty; // Libère l'ancienne cellule
        _playerPosition = target;
        _grid[_playerPosition.x, _playerPosition.y].State = CellState.Player; // Occupe la nouvelle cellule

        AudioManager.Instance?.PlaySFX(_moveClip);
        OnPlayerMoved.Invoke(_playerPosition);
    }

    private void HandleTurnProcessed() // Exécute la direction mise en file
    {
        if (_pendingDirection == null)
            return;

        Vector2Int queued = _pendingDirection.Value;
        _pendingDirection = null;
        TryMove(queued);
    }

    private bool IsInsideGrid(int col, int row) // Vérifie que les coordonnées sont valides
    {
        return col >= 0 && col < _columns && row >= 0 && row < _rows;
    }
}
