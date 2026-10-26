using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Controllers;

namespace TicTacToe.Tests;

public class GamesControllerTests
{
    [Fact]
    public void ControllerUsesApiControllerAttributeAndApiRoute()
    {
        var controllerType = typeof(GamesController);

        Assert.NotNull(controllerType.GetCustomAttribute<ApiControllerAttribute>());
        Assert.Equal("api/games", controllerType.GetCustomAttribute<RouteAttribute>()?.Template);
    }
}
