using TicTacToe.Application;
using TicTacToe.Domain;

namespace TicTacToe.Tests;

public class GameServiceTests
{
    private static readonly Guid SessionId = Guid.NewGuid();

    [Fact]
    public void ComputerReplyIsIncludedInTheMoveResponse()
    {
        var service = new GameService();
        var game = service.CreateGame(SessionId, GameMode.Computer);

        var updatedGame = service.MakeMove(SessionId, game.GameId, Player.X, 0, 0);

        Assert.Equal(2, updatedGame.Moves.Count);
        Assert.Equal(Player.X, updatedGame.Moves[0].Player);
        Assert.Equal(Player.O, updatedGame.Moves[1].Player);
        Assert.Equal(Player.X, updatedGame.CurrentPlayer);
        Assert.Equal(Player.O, updatedGame.Board[1][1]);
        Assert.Equal(
            new ScoreboardsResponse(new ScoreboardResponse(0, 0, 0), new ScoreboardResponse(0, 0, 0)),
            updatedGame.Scoreboard);
    }

    [Fact]
    public void TwoPlayerWinUpdatesOnlyTwoPlayerScoreboardAndResetGameLeavesScoresUnchanged()
    {
        var service = new GameService();
        var game = service.CreateGame(SessionId, GameMode.TwoPlayer);
        var result = CompleteTwoPlayerXWin(service, game.GameId);
        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(
            new ScoreboardsResponse(new ScoreboardResponse(1, 0, 0), new ScoreboardResponse(0, 0, 0)),
            result.Scoreboard);
        Assert.Throws<GameRuleException>(() =>
            service.MakeMove(SessionId, game.GameId, Player.O, 2, 2));
        Assert.Equal(new ScoreboardResponse(1, 0, 0), service.GetScoreboards(SessionId).TwoPlayer);
        Assert.Equal(new ScoreboardResponse(0, 0, 0), service.GetScoreboards(SessionId).Computer);

        var resetGame = service.ResetGame(SessionId, game.GameId);

        Assert.Equal(GameStatus.InProgress, resetGame.Status);
        Assert.Equal(result.Scoreboard, resetGame.Scoreboard);
    }

    [Fact]
    public void UnknownGameThrowsNotFoundException()
    {
        var service = new GameService();

        Assert.Throws<GameNotFoundException>(() => service.GetGame(SessionId, Guid.NewGuid()));
    }

    [Fact]
    public void ComputerWinUpdatesOnlyComputerScoreboard()
    {
        var service = new GameService();
        var game = service.CreateGame(SessionId, GameMode.Computer);

        service.MakeMove(SessionId, game.GameId, Player.X, 0, 0);
        service.MakeMove(SessionId, game.GameId, Player.X, 0, 1);
        var result = service.MakeMove(SessionId, game.GameId, Player.X, 2, 2);

        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(Player.O, result.Winner);
        Assert.Equal(
            new ScoreboardsResponse(new ScoreboardResponse(0, 0, 0), new ScoreboardResponse(0, 1, 0)),
            result.Scoreboard);
    }

    [Fact]
    public void ResettingOneScoreboardLeavesTheOtherUnchanged()
    {
        var service = new GameService();
        var twoPlayerGame = service.CreateGame(SessionId, GameMode.TwoPlayer);
        CompleteTwoPlayerXWin(service, twoPlayerGame.GameId);
        var computerGame = service.CreateGame(SessionId, GameMode.Computer);
        service.MakeMove(SessionId, computerGame.GameId, Player.X, 0, 0);
        service.MakeMove(SessionId, computerGame.GameId, Player.X, 0, 1);
        service.MakeMove(SessionId, computerGame.GameId, Player.X, 2, 2);

        var resetScoreboards = service.ResetScoreboard(SessionId, GameMode.TwoPlayer);

        Assert.Equal(
            new ScoreboardsResponse(new ScoreboardResponse(0, 0, 0), new ScoreboardResponse(0, 1, 0)),
            resetScoreboards);
    }

    [Fact]
    public void RepeatedGameReadsDoNotRecordAFinishedResultAgain()
    {
        var service = new GameService();
        var game = service.CreateGame(SessionId, GameMode.TwoPlayer);
        CompleteTwoPlayerXWin(service, game.GameId);

        service.GetGame(SessionId, game.GameId);
        service.GetGame(SessionId, game.GameId);
        var scoreboards = service.GetScoreboards(SessionId);

        Assert.Equal(new ScoreboardResponse(1, 0, 0), scoreboards.TwoPlayer);
        Assert.Equal(new ScoreboardResponse(0, 0, 0), scoreboards.Computer);
    }

    [Fact]
    public void ScoreboardsAndGamesAreIsolatedBetweenSessions()
    {
        var service = new GameService();
        var otherSessionId = Guid.NewGuid();
        var game = service.CreateGame(SessionId, GameMode.TwoPlayer);
        CompleteTwoPlayerXWin(service, game.GameId);

        Assert.Equal(
            new ScoreboardResponse(1, 0, 0),
            service.GetScoreboards(SessionId).TwoPlayer);
        Assert.Equal(
            new ScoreboardResponse(0, 0, 0),
            service.GetScoreboards(otherSessionId).TwoPlayer);
        Assert.Throws<GameNotFoundException>(() => service.GetGame(otherSessionId, game.GameId));

        service.ResetScoreboard(otherSessionId, GameMode.TwoPlayer);

        Assert.Equal(
            new ScoreboardResponse(1, 0, 0),
            service.GetScoreboards(SessionId).TwoPlayer);
    }

    private static GameStateResponse CompleteTwoPlayerXWin(GameService service, Guid gameId)
    {
        var moves = new (Player Player, int Row, int Column)[]
        {
            (Player.X, 0, 0),
            (Player.O, 1, 0),
            (Player.X, 0, 1),
            (Player.O, 1, 1),
            (Player.X, 0, 2)
        };

        GameStateResponse? result = null;
        foreach (var move in moves)
        {
            result = service.MakeMove(SessionId, gameId, move.Player, move.Row, move.Column);
        }

        return result!;
    }
}
