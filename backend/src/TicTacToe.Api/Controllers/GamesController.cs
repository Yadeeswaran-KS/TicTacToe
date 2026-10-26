using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api;
using TicTacToe.Api.Contracts;
using TicTacToe.Application;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/games")]
public sealed class GamesController(GameService gameService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<GameStateResponse>(StatusCodes.Status201Created)]
    public ActionResult<GameStateResponse> Create([FromBody] CreateGameRequest request)
    {
        var game = gameService.CreateGame(Request.GetSessionId(), request.Mode!.Value);
        return CreatedAtAction(nameof(GetById), new { id = game.GameId }, game);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<GameStateResponse>(StatusCodes.Status200OK)]
    public ActionResult<GameStateResponse> GetById(Guid id) =>
        Ok(gameService.GetGame(Request.GetSessionId(), id));

    [HttpPost("{id:guid}/moves")]
    [ProducesResponseType<GameStateResponse>(StatusCodes.Status200OK)]
    public ActionResult<GameStateResponse> MakeMove(
        Guid id,
        [FromBody] MakeMoveRequest request) =>
        Ok(gameService.MakeMove(
            Request.GetSessionId(),
            id,
            request.Player!.Value,
            request.Row!.Value,
            request.Column!.Value));

    [HttpPost("{id:guid}/undo")]
    [ProducesResponseType<GameStateResponse>(StatusCodes.Status200OK)]
    public ActionResult<GameStateResponse> Undo(Guid id) =>
        Ok(gameService.Undo(Request.GetSessionId(), id));

    [HttpPost("{id:guid}/reset")]
    [ProducesResponseType<GameStateResponse>(StatusCodes.Status200OK)]
    public ActionResult<GameStateResponse> Reset(Guid id) =>
        Ok(gameService.ResetGame(Request.GetSessionId(), id));
}
