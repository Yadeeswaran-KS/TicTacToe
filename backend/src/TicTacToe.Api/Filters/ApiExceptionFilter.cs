using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TicTacToe.Application;
using TicTacToe.Domain;

namespace TicTacToe.Api.Filters;

public sealed class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var (status, title, detail, reasonCode) = context.Exception switch
        {
            InvalidSessionIdException exception =>
                (StatusCodes.Status400BadRequest, "Invalid session", exception.Message, (string?)null),
            GameNotFoundException exception =>
                (StatusCodes.Status404NotFound, "Game not found", exception.Message, (string?)null),
            GameRuleException exception when exception.ReasonCode == GameRuleReason.OutOfBounds =>
                (StatusCodes.Status400BadRequest, "Invalid move", exception.Message, exception.ReasonCode.ToString()),
            GameRuleException exception =>
                (StatusCodes.Status409Conflict, "Game rule violation", exception.Message, exception.ReasonCode.ToString()),
            _ => (0, string.Empty, string.Empty, null)
        };

        if (status == 0)
        {
            return;
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        };
        if (reasonCode is not null)
        {
            problem.Extensions["reasonCode"] = reasonCode;
        }

        context.Result = new ObjectResult(problem)
        {
            StatusCode = status
        };
        context.ExceptionHandled = true;
    }
}
