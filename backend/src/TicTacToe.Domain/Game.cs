namespace TicTacToe.Domain;

public sealed class Game
{
    private readonly List<MoveRecord> _moves = [];

    public Game(GameMode mode)
    {
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode));
        }

        Mode = mode;
    }

    public GameMode Mode { get; }

    public bool ScoreRecorded { get; private set; }

    public IReadOnlyList<MoveRecord> MoveHistory => _moves.AsReadOnly();

    public IReadOnlyList<Player?> Board =>
        Array.AsReadOnly(ReplayMoves().Board);

    public Player? CurrentPlayer => ReplayMoves().CurrentPlayer;

    public GameStatus Status => ReplayMoves().Status;

    public Player? Winner => ReplayMoves().Winner;

    public IReadOnlyList<int> WinningCells =>
        Array.AsReadOnly(ReplayMoves().WinningCells);

    public bool CanUndo =>
        ReplayMoves().Status == GameStatus.InProgress && _moves.Count > 0;

    public void MakeMove(Player player, int cellIndex)
    {
        var state = ReplayMoves();
        if (state.Status != GameStatus.InProgress)
        {
            throw new GameRuleException(GameRuleReason.GameOver);
        }

        if (cellIndex is < 0 or >= 9)
        {
            throw new GameRuleException(GameRuleReason.OutOfBounds);
        }

        if (state.Board[cellIndex] is not null)
        {
            throw new GameRuleException(GameRuleReason.CellOccupied);
        }

        if (player != state.CurrentPlayer
            || (Mode == GameMode.Computer && player != Player.X))
        {
            throw new GameRuleException(GameRuleReason.WrongPlayer);
        }

        _moves.Add(new MoveRecord(player, cellIndex));

        if (Mode == GameMode.Computer && ReplayMoves().Status == GameStatus.InProgress)
        {
            var computerCell = ComputerPlayer.ChooseMove(ReplayMoves().Board);
            _moves.Add(new MoveRecord(Player.O, computerCell));
        }
    }

    public void Undo()
    {
        var state = ReplayMoves();
        if (state.Status != GameStatus.InProgress)
        {
            throw new GameRuleException(GameRuleReason.UndoNotAllowed);
        }

        if (_moves.Count == 0)
        {
            throw new GameRuleException(GameRuleReason.NothingToUndo);
        }

        var lastMove = _moves[^1];
        _moves.RemoveAt(_moves.Count - 1);

        if (Mode == GameMode.Computer
            && lastMove.Player == Player.O
            && _moves.Count > 0
            && _moves[^1].Player == Player.X)
        {
            _moves.RemoveAt(_moves.Count - 1);
        }
    }

    public void Reset()
    {
        _moves.Clear();
        ScoreRecorded = false;
    }

    public bool TryMarkScoreRecorded()
    {
        if (ScoreRecorded)
        {
            return false;
        }

        ScoreRecorded = true;
        return true;
    }

    private GameState ReplayMoves()
    {
        var board = new Player?[9];
        Player? winner = null;
        int[] winningCells = [];
        var status = GameStatus.InProgress;
        Player? currentPlayer = Player.X;

        foreach (var move in _moves)
        {
            board[move.CellIndex] = move.Player;
            var winningLine = BoardRules.FindWinningLine(board, move.Player);
            if (winningLine is not null)
            {
                status = GameStatus.Won;
                winner = move.Player;
                winningCells = winningLine;
                currentPlayer = null;
                break;
            }

            if (board.All(cell => cell is not null))
            {
                status = GameStatus.Draw;
                currentPlayer = null;
                break;
            }

            currentPlayer = move.Player == Player.X ? Player.O : Player.X;
        }

        return new GameState(board, currentPlayer, status, winner, winningCells);
    }
    private sealed record GameState(
        Player?[] Board,
        Player? CurrentPlayer,
        GameStatus Status,
        Player? Winner,
        int[] WinningCells);
}
