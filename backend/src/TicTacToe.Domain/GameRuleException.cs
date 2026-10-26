namespace TicTacToe.Domain;

public enum GameRuleReason
{
    OutOfBounds,
    CellOccupied,
    GameOver,
    WrongPlayer,
    NothingToUndo,
    UndoNotAllowed
}

public sealed class GameRuleException : Exception
{
    public GameRuleException(GameRuleReason reasonCode)
        : base($"The game operation is not allowed: {reasonCode}.")
    {
        ReasonCode = reasonCode;
    }

    public GameRuleReason ReasonCode { get; }
}
