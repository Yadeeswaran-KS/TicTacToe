using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TicTacToe.Tests;

public sealed class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Session-Id", Guid.NewGuid().ToString());
    }

    [Fact]
    public async Task ScoreboardAndGamesAreIsolatedBetweenSessions()
    {
        var gameResponse = await _client.PostAsJsonAsync(
            "/api/games",
            new { mode = "TwoPlayer" });
        using var gameJson = JsonDocument.Parse(await gameResponse.Content.ReadAsStringAsync());
        var gameId = gameJson.RootElement.GetProperty("gameId").GetGuid();
        var moves = new (string Player, int Row, int Column)[]
        {
            ("X", 0, 0),
            ("O", 1, 0),
            ("X", 0, 1),
            ("O", 1, 1),
            ("X", 0, 2)
        };
        foreach (var move in moves)
        {
            await _client.PostAsJsonAsync(
                $"/api/games/{gameId}/moves",
                new { player = move.Player, row = move.Row, column = move.Column });
        }

        var ownScoreboardResponse = await _client.GetAsync("/api/scoreboard");
        using var ownScoreboardJson = JsonDocument.Parse(
            await ownScoreboardResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            1,
            ownScoreboardJson.RootElement.GetProperty("twoPlayer").GetProperty("xWins").GetInt32());

        using var otherSession = _factory.CreateClient();
        otherSession.DefaultRequestHeaders.Add("X-Session-Id", Guid.NewGuid().ToString());

        var otherSessionScoreboard = await otherSession.GetAsync("/api/scoreboard");
        Assert.Equal(HttpStatusCode.OK, otherSessionScoreboard.StatusCode);
        using var scoreboardJson = JsonDocument.Parse(
            await otherSessionScoreboard.Content.ReadAsStringAsync());
        Assert.Equal(
            0,
            scoreboardJson.RootElement.GetProperty("twoPlayer").GetProperty("xWins").GetInt32());

        var gameFromOtherSession = await otherSession.GetAsync($"/api/games/{gameId}");
        Assert.Equal(HttpStatusCode.NotFound, gameFromOtherSession.StatusCode);

        var resetOtherScoreboard = await otherSession.PostAsync(
            "/api/scoreboard/reset?mode=TwoPlayer",
            null);
        Assert.Equal(HttpStatusCode.OK, resetOtherScoreboard.StatusCode);
        var ownScoreboardAfterOtherReset = await _client.GetAsync("/api/scoreboard");
        using var ownScoreboardAfterOtherResetJson = JsonDocument.Parse(
            await ownScoreboardAfterOtherReset.Content.ReadAsStringAsync());
        Assert.Equal(
            1,
            ownScoreboardAfterOtherResetJson.RootElement
                .GetProperty("twoPlayer")
                .GetProperty("xWins")
                .GetInt32());
    }

    [Fact]
    public async Task MissingSessionHeaderReturnsBadRequest()
    {
        using var clientWithoutSession = _factory.CreateClient();

        var response = await clientWithoutSession.GetAsync("/api/scoreboard");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Invalid session", problem.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task CreateComputerGameAndMoveReturnsGameStateWithAutomaticReply()
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/games",
            new { mode = "Computer" });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        using var createdJson = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var gameId = createdJson.RootElement.GetProperty("gameId").GetGuid();
        Assert.Equal("Computer", createdJson.RootElement.GetProperty("mode").GetString());
        Assert.Equal("X", createdJson.RootElement.GetProperty("currentPlayer").GetString());
        Assert.Equal(3, createdJson.RootElement.GetProperty("board").GetArrayLength());

        var moveResponse = await _client.PostAsJsonAsync(
            $"/api/games/{gameId}/moves",
            new { player = "X", row = 0, column = 0 });

        Assert.Equal(HttpStatusCode.OK, moveResponse.StatusCode);
        using var movedJson = JsonDocument.Parse(await moveResponse.Content.ReadAsStringAsync());
        var root = movedJson.RootElement;
        var moves = root.GetProperty("moves");
        Assert.Equal(2, moves.GetArrayLength());
        Assert.Equal("X", moves[0].GetProperty("player").GetString());
        Assert.Equal(1, moves[0].GetProperty("moveNumber").GetInt32());
        Assert.Equal("O", moves[1].GetProperty("player").GetString());
        Assert.Equal(2, moves[1].GetProperty("moveNumber").GetInt32());
        Assert.Equal("O", root.GetProperty("board")[1][1].GetString());
        Assert.Equal("X", root.GetProperty("currentPlayer").GetString());
        Assert.Equal(0, root.GetProperty("scoreboard").GetProperty("twoPlayer").GetProperty("xWins").GetInt32());
        Assert.Equal(0, root.GetProperty("scoreboard").GetProperty("computer").GetProperty("xWins").GetInt32());
    }

    [Fact]
    public async Task ScoreboardEndpointsReturnBothBoardsAndResetOnlyTheSelectedMode()
    {
        var twoPlayerGame = await CreateTwoPlayerXWin();
        var computerGame = await CreateComputerOWin();
        Assert.Equal(HttpStatusCode.OK, twoPlayerGame.StatusCode);
        Assert.Equal(HttpStatusCode.OK, computerGame.StatusCode);

        using var gameJson = JsonDocument.Parse(await twoPlayerGame.Content.ReadAsStringAsync());
        var gameScoreboards = gameJson.RootElement.GetProperty("scoreboard");
        Assert.Equal(1, gameScoreboards.GetProperty("twoPlayer").GetProperty("xWins").GetInt32());
        Assert.Equal(0, gameScoreboards.GetProperty("computer").GetProperty("oWins").GetInt32());

        using var computerJson = JsonDocument.Parse(await computerGame.Content.ReadAsStringAsync());
        gameScoreboards = computerJson.RootElement.GetProperty("scoreboard");
        Assert.Equal(1, gameScoreboards.GetProperty("computer").GetProperty("oWins").GetInt32());

        var response = await _client.PostAsync("/api/scoreboard/reset?mode=TwoPlayer", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var resetJson = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(0, resetJson.RootElement.GetProperty("twoPlayer").GetProperty("xWins").GetInt32());
        Assert.Equal(1, resetJson.RootElement.GetProperty("computer").GetProperty("oWins").GetInt32());

        var getResponse = await _client.GetAsync("/api/scoreboard");
        using var getJson = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());
        Assert.Equal(0, getJson.RootElement.GetProperty("twoPlayer").GetProperty("xWins").GetInt32());
        Assert.Equal(1, getJson.RootElement.GetProperty("computer").GetProperty("oWins").GetInt32());

    }

    [Theory]
    [InlineData("/api/scoreboard/reset")]
    [InlineData("/api/scoreboard/reset?mode=Invalid")]
    [InlineData("/api/scoreboard/reset?mode=0")]
    public async Task ScoreboardResetWithoutAValidModeReturnsProblemDetails(string path)
    {
        var response = await _client.PostAsync(path, null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(400, problem.RootElement.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task UnknownGameReturnsProblemDetailsNotFound()
    {
        var response = await _client.GetAsync(
            "/api/games/00000000-0000-0000-0000-000000000000");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(404, problem.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Game not found", problem.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task OffBoardAndOtherRuleViolationsReturnExpectedStatuses()
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/games",
            new { mode = "TwoPlayer" });
        using var createdJson = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var gameId = createdJson.RootElement.GetProperty("gameId").GetGuid();

        var offBoardResponse = await _client.PostAsJsonAsync(
            $"/api/games/{gameId}/moves",
            new { player = "X", row = 3, column = 0 });
        Assert.Equal(HttpStatusCode.BadRequest, offBoardResponse.StatusCode);

        var firstMoveResponse = await _client.PostAsJsonAsync(
            $"/api/games/{gameId}/moves",
            new { player = "X", row = 0, column = 0 });
        Assert.Equal(HttpStatusCode.OK, firstMoveResponse.StatusCode);

        var wrongPlayerResponse = await _client.PostAsJsonAsync(
            $"/api/games/{gameId}/moves",
            new { player = "X", row = 0, column = 1 });
        Assert.Equal(HttpStatusCode.Conflict, wrongPlayerResponse.StatusCode);

        var occupiedResponse = await _client.PostAsJsonAsync(
            $"/api/games/{gameId}/moves",
            new { player = "O", row = 0, column = 0 });
        Assert.Equal(HttpStatusCode.Conflict, occupiedResponse.StatusCode);
    }

    private async Task<HttpResponseMessage> CreateTwoPlayerXWin()
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/games",
            new { mode = "TwoPlayer" });
        using var createdJson = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var gameId = createdJson.RootElement.GetProperty("gameId").GetGuid();
        var moves = new (string Player, int Row, int Column)[]
        {
            ("X", 0, 0),
            ("O", 1, 0),
            ("X", 0, 1),
            ("O", 1, 1),
            ("X", 0, 2)
        };

        HttpResponseMessage? response = null;
        foreach (var move in moves)
        {
            response = await _client.PostAsJsonAsync(
                $"/api/games/{gameId}/moves",
                new { player = move.Player, row = move.Row, column = move.Column });
        }

        return response!;
    }

    private async Task<HttpResponseMessage> CreateComputerOWin()
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/games",
            new { mode = "Computer" });
        using var createdJson = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var gameId = createdJson.RootElement.GetProperty("gameId").GetGuid();

        await _client.PostAsJsonAsync(
            $"/api/games/{gameId}/moves",
            new { player = "X", row = 0, column = 0 });
        await _client.PostAsJsonAsync(
            $"/api/games/{gameId}/moves",
            new { player = "X", row = 0, column = 1 });
        return await _client.PostAsJsonAsync(
            $"/api/games/{gameId}/moves",
            new { player = "X", row = 2, column = 2 });
    }
}
