using System.ComponentModel.DataAnnotations;
using TicTacToe.Domain;

namespace TicTacToe.Api.Contracts;

public sealed class MakeMoveRequest
{
    [Required]
    [EnumDataType(typeof(Player))]
    public Player? Player { get; init; }

    [Required]
    public int? Row { get; init; }

    [Required]
    public int? Column { get; init; }
}
