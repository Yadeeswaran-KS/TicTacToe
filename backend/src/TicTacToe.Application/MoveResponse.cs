using TicTacToe.Domain;

namespace TicTacToe.Application;

public sealed record MoveResponse(int MoveNumber, Player Player, int Row, int Column);
