namespace TicTacToe.Domain;

public sealed class Scoreboard
{
    public int XWins { get; private set; }

    public int OWins { get; private set; }

    public int Draws { get; private set; }

    public void RecordWin(Player player)
    {
        switch (player)
        {
            case Player.X:
                XWins++;
                break;
            case Player.O:
                OWins++;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(player));
        }
    }

    public void RecordDraw()
    {
        Draws++;
    }

    public void Reset()
    {
        XWins = 0;
        OWins = 0;
        Draws = 0;
    }
}
