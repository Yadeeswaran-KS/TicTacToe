namespace TicTacToe.Api;

public static class SessionIdRequestExtensions
{
    public static Guid GetSessionId(this HttpRequest request)
    {
        var headerValue = request.Headers["X-Session-Id"].ToString();
        if (!Guid.TryParse(headerValue, out var sessionId) || sessionId == Guid.Empty)
        {
            throw new InvalidSessionIdException();
        }

        return sessionId;
    }
}

public sealed class InvalidSessionIdException : Exception
{
    public InvalidSessionIdException()
        : base("A valid X-Session-Id header is required.")
    {
    }
}
