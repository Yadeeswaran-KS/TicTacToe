using TicTacToe.Domain;

namespace TicTacToe.Application;

public sealed class GameService
{
    private readonly object _sync = new();
    private readonly Dictionary<Guid, GameEntry> _games = [];
    private readonly Dictionary<Guid, Dictionary<GameMode, Scoreboard>> _scoreboards = [];

    public GameStateResponse CreateGame(Guid sessionId, GameMode mode)
    {
        lock (_sync)
        {
            var game = new Game(mode);
            var scoreboards = GetScoreboardsOrCreate(sessionId);
            var gameId = Guid.NewGuid();
            _games.Add(gameId, new GameEntry(sessionId, game));
            return CreateGameState(gameId, game, scoreboards);
        }
    }

    public GameStateResponse GetGame(Guid sessionId, Guid gameId)
    {
        lock (_sync)
        {
            var game = GetGameOrThrow(sessionId, gameId);
            return CreateGameState(gameId, game, GetScoreboardsOrCreate(sessionId));
        }
    }

    public GameStateResponse MakeMove(Guid sessionId, Guid gameId, Player player, int row, int column)
    {
        lock (_sync)
        {
            var game = GetGameOrThrow(sessionId, gameId);
            var scoreboards = GetScoreboardsOrCreate(sessionId);
            if (row is < 0 or >= 3 || column is < 0 or >= 3)
            {
                throw new GameRuleException(GameRuleReason.OutOfBounds);
            }

            game.MakeMove(player, row * 3 + column);
            UpdateScoreboardIfFinished(game, scoreboards);
            return CreateGameState(gameId, game, scoreboards);
        }
    }

    public GameStateResponse Undo(Guid sessionId, Guid gameId)
    {
        lock (_sync)
        {
            var game = GetGameOrThrow(sessionId, gameId);
            var scoreboards = GetScoreboardsOrCreate(sessionId);
            game.Undo();
            return CreateGameState(gameId, game, scoreboards);
        }
    }

    public GameStateResponse ResetGame(Guid sessionId, Guid gameId)
    {
        lock (_sync)
        {
            var game = GetGameOrThrow(sessionId, gameId);
            var scoreboards = GetScoreboardsOrCreate(sessionId);
            game.Reset();
            return CreateGameState(gameId, game, scoreboards);
        }
    }

    public ScoreboardsResponse GetScoreboards(Guid sessionId)
    {
        lock (_sync)
        {
            return CreateScoreboardsResponse(GetScoreboardsOrCreate(sessionId));
        }
    }

    public ScoreboardsResponse ResetScoreboard(Guid sessionId, GameMode mode)
    {
        lock (_sync)
        {
            if (!Enum.IsDefined(mode))
            {
                throw new ArgumentOutOfRangeException(nameof(mode));
            }

            var scoreboards = GetScoreboardsOrCreate(sessionId);
            GetScoreboardOrThrow(scoreboards, mode).Reset();
            return CreateScoreboardsResponse(scoreboards);
        }
    }

    private Game GetGameOrThrow(Guid sessionId, Guid gameId) =>
        _games.TryGetValue(gameId, out var entry) && entry.SessionId == sessionId
            ? entry.Game
            : throw new GameNotFoundException(gameId);

    private void UpdateScoreboardIfFinished(
        Game game,
        IReadOnlyDictionary<GameMode, Scoreboard> scoreboards)
    {
        if (game.Status == GameStatus.Won && game.Winner is { } winner
            && game.TryMarkScoreRecorded())
        {
            scoreboards[game.Mode].RecordWin(winner);
        }
        else if (game.Status == GameStatus.Draw && game.TryMarkScoreRecorded())
        {
            scoreboards[game.Mode].RecordDraw();
        }
    }

    private GameStateResponse CreateGameState(
        Guid gameId,
        Game game,
        IReadOnlyDictionary<GameMode, Scoreboard> scoreboards)
    {
        var board = game.Board;
        var boardRows = Enumerable.Range(0, 3)
            .Select(row => (IReadOnlyList<Player?>)Array.AsReadOnly(
                board.Skip(row * 3).Take(3).ToArray()))
            .ToArray();
        var moves = game.MoveHistory
            .Select((move, index) => new MoveResponse(
                index + 1,
                move.Player,
                move.CellIndex / 3,
                move.CellIndex % 3))
            .ToArray();

        return new GameStateResponse(
            gameId,
            game.Mode,
            game.Status,
            game.CurrentPlayer,
            game.Winner,
            Array.AsReadOnly(game.WinningCells.ToArray()),
            Array.AsReadOnly(boardRows),
            Array.AsReadOnly(moves),
            game.CanUndo,
            CreateScoreboardsResponse(scoreboards));
    }

    private static Scoreboard GetScoreboardOrThrow(
        IReadOnlyDictionary<GameMode, Scoreboard> scoreboards,
        GameMode mode) =>
        scoreboards.TryGetValue(mode, out var scoreboard)
            ? scoreboard
            : throw new ArgumentOutOfRangeException(nameof(mode));

    private Dictionary<GameMode, Scoreboard> GetScoreboardsOrCreate(Guid sessionId)
    {
        if (sessionId == Guid.Empty)
        {
            throw new ArgumentException("A non-empty session ID is required.", nameof(sessionId));
        }

        if (!_scoreboards.TryGetValue(sessionId, out var scoreboards))
        {
            scoreboards = Enum.GetValues<GameMode>()
                .ToDictionary(mode => mode, _ => new Scoreboard());
            _scoreboards.Add(sessionId, scoreboards);
        }

        return scoreboards;
    }

    private static ScoreboardsResponse CreateScoreboardsResponse(
        IReadOnlyDictionary<GameMode, Scoreboard> scoreboards) =>
        new(
            CreateScoreboardResponse(scoreboards[GameMode.TwoPlayer]),
            CreateScoreboardResponse(scoreboards[GameMode.Computer]));

    private static ScoreboardResponse CreateScoreboardResponse(Scoreboard scoreboard) =>
        new(scoreboard.XWins, scoreboard.OWins, scoreboard.Draws);

    private sealed record GameEntry(Guid SessionId, Game Game);
}
