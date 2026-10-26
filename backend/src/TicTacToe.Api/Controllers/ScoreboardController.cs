using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api;
using TicTacToe.Application;
using TicTacToe.Domain;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/scoreboard")]
public sealed class ScoreboardController(GameService gameService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ScoreboardsResponse>(StatusCodes.Status200OK)]
    public ActionResult<ScoreboardsResponse> Get() =>
        Ok(gameService.GetScoreboards(Request.GetSessionId()));

    [HttpPost("reset")]
    [ProducesResponseType<ScoreboardsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<ScoreboardsResponse> Reset([FromQuery] string? mode)
    {
        var gameMode = mode switch
        {
            nameof(GameMode.TwoPlayer) => GameMode.TwoPlayer,
            nameof(GameMode.Computer) => GameMode.Computer,
            _ => (GameMode?)null
        };

        if (gameMode is null)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid scoreboard mode",
                Detail = "The mode query parameter must be either 'TwoPlayer' or 'Computer'."
            });
        }

        return Ok(gameService.ResetScoreboard(Request.GetSessionId(), gameMode.Value));
    }
}
