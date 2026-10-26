namespace TicTacToe.Domain;

internal static class BoardRules
{
    private static readonly int[][] WinningLines =
    [
        [0, 1, 2],
        [3, 4, 5],
        [6, 7, 8],
        [0, 3, 6],
        [1, 4, 7],
        [2, 5, 8],
        [0, 4, 8],
        [2, 4, 6]
    ];

    internal static int[]? FindWinningLine(IReadOnlyList<Player?> board, Player player) =>
        WinningLines.FirstOrDefault(line => line.All(index => board[index] == player));
}
