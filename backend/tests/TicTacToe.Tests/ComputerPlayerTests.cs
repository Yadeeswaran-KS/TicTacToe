using TicTacToe.Domain;

namespace TicTacToe.Tests;

public class ComputerPlayerTests
{
    [Fact]
    public void WinTakesPriorityOverBlock()
    {
        Player?[] board =
        [
            Player.O, Player.O, null,
            Player.X, Player.X, null,
            null, null, null
        ];

        Assert.Equal(2, ComputerPlayer.ChooseMove(board));
    }

    [Fact]
    public void BlocksHumanWinningMove()
    {
        Player?[] board =
        [
            Player.X, Player.X, null,
            null, Player.O, null,
            null, null, null
        ];

        Assert.Equal(2, ComputerPlayer.ChooseMove(board));
    }

    [Fact]
    public void ChoosesCentreBeforeCorner()
    {
        Player?[] board = new Player?[9];

        Assert.Equal(4, ComputerPlayer.ChooseMove(board));
    }

    [Fact]
    public void ChoosesFirstAvailableCornerWhenCentreIsOccupied()
    {
        Player?[] boardWithCornersFree = new Player?[9];
        boardWithCornersFree[4] = Player.X;
        Assert.Equal(0, ComputerPlayer.ChooseMove(boardWithCornersFree));
    }

    [Fact]
    public void ChoosesFirstAvailableCellWhenNoCornersOrCentreAreFree()
    {
        Player?[] board =
        [
            Player.X, null, Player.O,
            Player.O, Player.X, Player.X,
            Player.X, Player.O, Player.O
        ];

        Assert.Equal(1, ComputerPlayer.ChooseMove(board));
    }
}
