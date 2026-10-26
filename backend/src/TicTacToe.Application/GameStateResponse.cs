using TicTacToe.Domain;

namespace TicTacToe.Application;

public sealed record GameStateResponse(
    Guid GameId,
    GameMode Mode,
    GameStatus Status,
    Player? CurrentPlayer,
    Player? Winner,
    IReadOnlyList<int> WinningCells,
    IReadOnlyList<IReadOnlyList<Player?>> Board,
    IReadOnlyList<MoveResponse> Moves,
    bool CanUndo,
    ScoreboardsResponse Scoreboard);
