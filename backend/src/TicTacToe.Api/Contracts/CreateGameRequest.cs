using System.ComponentModel.DataAnnotations;
using TicTacToe.Domain;

namespace TicTacToe.Api.Contracts;

public sealed class CreateGameRequest
{
    [Required]
    [EnumDataType(typeof(GameMode))]
    public GameMode? Mode { get; init; }
}
