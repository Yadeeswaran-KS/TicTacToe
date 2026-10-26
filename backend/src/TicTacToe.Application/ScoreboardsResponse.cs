namespace TicTacToe.Application;

public sealed record ScoreboardsResponse(
    ScoreboardResponse TwoPlayer,
    ScoreboardResponse Computer);
