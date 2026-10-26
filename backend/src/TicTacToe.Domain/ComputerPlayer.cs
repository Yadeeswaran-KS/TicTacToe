namespace TicTacToe.Domain;

public static class ComputerPlayer
{
    private static readonly int[] Corners = [0, 2, 6, 8];

    public static int ChooseMove(IReadOnlyList<Player?> board)
    {
        ArgumentNullException.ThrowIfNull(board);

        if (board.Count != 9)
        {
            throw new ArgumentException("A Tic Tac Toe board must contain 9 cells.", nameof(board));
        }

        var winningMove = FindWinningMove(board, Player.O)
            ?? FindWinningMove(board, Player.X);
        if (winningMove is not null)
        {
            return winningMove.Value;
        }

        return board[4] is null ? 4 : ChooseCornerOrFreeCell(board);
    }

    private static int? FindWinningMove(IReadOnlyList<Player?> board, Player player)
    {
        for (var cellIndex = 0; cellIndex < board.Count; cellIndex++)
        {
            if (board[cellIndex] is null)
            {
                var candidate = board.ToArray();
                candidate[cellIndex] = player;
                if (BoardRules.FindWinningLine(candidate, player) is not null)
                {
                    return cellIndex;
                }
            }
        }

        return null;
    }

    private static int ChooseCornerOrFreeCell(IReadOnlyList<Player?> board)
    {
        foreach (var corner in Corners)
        {
            if (board[corner] is null)
            {
                return corner;
            }
        }

        for (var index = 0; index < board.Count; index++)
        {
            if (board[index] is null)
            {
                return index;
            }
        }

        throw new InvalidOperationException("There are no free cells on the board.");
    }
}
