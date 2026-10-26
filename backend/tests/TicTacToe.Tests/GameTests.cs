using TicTacToe.Domain;

namespace TicTacToe.Tests;

public class GameTests
{
    [Fact]
    public void NewGameStartsWithEmptyBoardAndX()
    {
        var game = new Game(GameMode.TwoPlayer);

        Assert.All(game.Board, cell => Assert.Null(cell));
        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.False(game.CanUndo);
    }

    [Fact]
    public void MakeMoveReplaysMovesToDeriveBoardAndTurn()
    {
        var game = new Game(GameMode.TwoPlayer);

        game.MakeMove(Player.X, 0);
        game.MakeMove(Player.O, 4);

        Assert.Equal(Player.X, game.Board[0]);
        Assert.Equal(Player.O, game.Board[4]);
        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Equal(2, game.MoveHistory.Count);
    }

    [Fact]
    public void InvalidMoveDoesNotChangeHistoryOrTurn()
    {
        var game = new Game(GameMode.TwoPlayer);
        game.MakeMove(Player.X, 0);

        var exception = Assert.Throws<GameRuleException>(() => game.MakeMove(Player.X, 1));

        Assert.Equal(GameRuleReason.WrongPlayer, exception.ReasonCode);
        Assert.Single(game.MoveHistory);
        Assert.Equal(Player.O, game.CurrentPlayer);
        Assert.Null(game.Board[1]);
    }

    [Fact]
    public void ComputerModeRejectsOAsTheHumanPlayer()
    {
        var game = new Game(GameMode.Computer);

        var exception = Assert.Throws<GameRuleException>(() => game.MakeMove(Player.O, 0));

        Assert.Equal(GameRuleReason.WrongPlayer, exception.ReasonCode);
        Assert.Empty(game.MoveHistory);
        Assert.Equal(Player.X, game.CurrentPlayer);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(9)]
    public void MakeMoveRejectsOutOfBoundsCell(int cellIndex)
    {
        var game = new Game(GameMode.TwoPlayer);

        var exception = Assert.Throws<GameRuleException>(() => game.MakeMove(Player.X, cellIndex));

        Assert.Equal(GameRuleReason.OutOfBounds, exception.ReasonCode);
        Assert.Empty(game.MoveHistory);
    }

    [Fact]
    public void MakeMoveRejectsOccupiedCell()
    {
        var game = new Game(GameMode.TwoPlayer);
        game.MakeMove(Player.X, 0);
        game.MakeMove(Player.O, 4);

        var exception = Assert.Throws<GameRuleException>(() => game.MakeMove(Player.X, 0));

        Assert.Equal(GameRuleReason.CellOccupied, exception.ReasonCode);
        Assert.Equal(2, game.MoveHistory.Count);
        Assert.Equal(Player.X, game.CurrentPlayer);
    }

    [Fact]
    public void WinAndWinningCellsAreDerivedFromMoveHistory()
    {
        var game = new Game(GameMode.TwoPlayer);
        game.MakeMove(Player.X, 0);
        game.MakeMove(Player.O, 3);
        game.MakeMove(Player.X, 1);
        game.MakeMove(Player.O, 4);
        game.MakeMove(Player.X, 2);

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(Player.X, game.Winner);
        Assert.Null(game.CurrentPlayer);
        Assert.Equal([0, 1, 2], game.WinningCells);
        Assert.False(game.CanUndo);
    }

    [Theory]
    [MemberData(nameof(WinningMoveSequences))]
    public void DetectsRowColumnAndDiagonalWins((Player Player, int Cell)[] moves, int[] winningCells)
    {
        var game = new Game(GameMode.TwoPlayer);

        foreach (var move in moves)
        {
            game.MakeMove(move.Player, move.Cell);
        }

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(Player.X, game.Winner);
        Assert.Equal(winningCells, game.WinningCells);
    }

    public static IEnumerable<object[]> WinningMoveSequences()
    {
        yield return
        [
            new (Player, int)[]
            {
                (Player.X, 0), (Player.O, 3),
                (Player.X, 1), (Player.O, 4),
                (Player.X, 2)
            },
            new[] { 0, 1, 2 }
        ];
        yield return
        [
            new (Player, int)[]
            {
                (Player.X, 0), (Player.O, 1),
                (Player.X, 3), (Player.O, 2),
                (Player.X, 6)
            },
            new[] { 0, 3, 6 }
        ];
        yield return
        [
            new (Player, int)[]
            {
                (Player.X, 0), (Player.O, 1),
                (Player.X, 4), (Player.O, 2),
                (Player.X, 8)
            },
            new[] { 0, 4, 8 }
        ];
    }

    [Fact]
    public void DrawIsDerivedFromMoveHistory()
    {
        var game = new Game(GameMode.TwoPlayer);
        var moves = new (Player Player, int Cell)[]
        {
            (Player.X, 0), (Player.O, 1),
            (Player.X, 2), (Player.O, 4),
            (Player.X, 3), (Player.O, 5),
            (Player.X, 7), (Player.O, 6),
            (Player.X, 8)
        };

        foreach (var move in moves)
        {
            game.MakeMove(move.Player, move.Cell);
        }

        Assert.Equal(GameStatus.Draw, game.Status);
        Assert.Null(game.Winner);
        Assert.Empty(game.WinningCells);
        Assert.False(game.CanUndo);
    }

    [Fact]
    public void UndoRemovesOneTwoPlayerMoveAndRecomputesState()
    {
        var game = new Game(GameMode.TwoPlayer);
        game.MakeMove(Player.X, 0);
        game.MakeMove(Player.O, 4);

        game.Undo();

        Assert.Single(game.MoveHistory);
        Assert.Null(game.Board[4]);
        Assert.Equal(Player.O, game.CurrentPlayer);
        Assert.True(game.CanUndo);
    }

    [Fact]
    public void UndoRemovesComputerReplyAndHumanMove()
    {
        var game = new Game(GameMode.Computer);
        game.MakeMove(Player.X, 0);

        Assert.Equal(2, game.MoveHistory.Count);
        Assert.Equal(Player.O, game.MoveHistory[1].Player);

        game.Undo();

        Assert.Empty(game.MoveHistory);
        Assert.All(game.Board, cell => Assert.Null(cell));
        Assert.Equal(Player.X, game.CurrentPlayer);
    }

    [Fact]
    public void UndoRejectsFinishedGames()
    {
        var game = new Game(GameMode.TwoPlayer);
        game.MakeMove(Player.X, 0);
        game.MakeMove(Player.O, 3);
        game.MakeMove(Player.X, 1);
        game.MakeMove(Player.O, 4);
        game.MakeMove(Player.X, 2);

        var exception = Assert.Throws<GameRuleException>(() => game.Undo());

        Assert.Equal(GameRuleReason.UndoNotAllowed, exception.ReasonCode);
        Assert.Equal(5, game.MoveHistory.Count);
    }

    [Fact]
    public void UndoRejectsDrawnGames()
    {
        var game = new Game(GameMode.TwoPlayer);
        var moves = new (Player Player, int Cell)[]
        {
            (Player.X, 0), (Player.O, 1),
            (Player.X, 2), (Player.O, 4),
            (Player.X, 3), (Player.O, 5),
            (Player.X, 7), (Player.O, 6),
            (Player.X, 8)
        };

        foreach (var move in moves)
        {
            game.MakeMove(move.Player, move.Cell);
        }

        var exception = Assert.Throws<GameRuleException>(() => game.Undo());

        Assert.Equal(GameRuleReason.UndoNotAllowed, exception.ReasonCode);
        Assert.Equal(9, game.MoveHistory.Count);
    }

    [Fact]
    public void MoveAfterCompletionReportsGameOver()
    {
        var game = new Game(GameMode.TwoPlayer);
        game.MakeMove(Player.X, 0);
        game.MakeMove(Player.O, 3);
        game.MakeMove(Player.X, 1);
        game.MakeMove(Player.O, 4);
        game.MakeMove(Player.X, 2);

        var exception = Assert.Throws<GameRuleException>(() => game.MakeMove(Player.O, 5));

        Assert.Equal(GameRuleReason.GameOver, exception.ReasonCode);
        Assert.Equal(5, game.MoveHistory.Count);
        Assert.Equal(GameStatus.Won, game.Status);
    }

    [Fact]
    public void UndoWithoutMovesReportsNothingToUndo()
    {
        var game = new Game(GameMode.TwoPlayer);

        var exception = Assert.Throws<GameRuleException>(() => game.Undo());

        Assert.Equal(GameRuleReason.NothingToUndo, exception.ReasonCode);
    }

    [Fact]
    public void ResetClearsMovesAndRestoresInitialDerivedState()
    {
        var game = new Game(GameMode.TwoPlayer);
        game.MakeMove(Player.X, 0);
        game.MakeMove(Player.O, 4);

        game.Reset();

        Assert.Empty(game.MoveHistory);
        Assert.All(game.Board, cell => Assert.Null(cell));
        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }
}
