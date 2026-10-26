using TicTacToe.Domain;

namespace TicTacToe.Tests;

public class ScoreboardTests
{
    [Fact]
    public void RecordsWinsAndDrawsAndCanReset()
    {
        var scoreboard = new Scoreboard();

        scoreboard.RecordWin(Player.X);
        scoreboard.RecordWin(Player.O);
        scoreboard.RecordDraw();
        scoreboard.Reset();

        Assert.Equal(0, scoreboard.XWins);
        Assert.Equal(0, scoreboard.OWins);
        Assert.Equal(0, scoreboard.Draws);
    }
}
