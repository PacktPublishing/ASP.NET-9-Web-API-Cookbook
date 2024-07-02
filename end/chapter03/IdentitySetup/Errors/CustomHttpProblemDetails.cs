using Microsoft.AspNetCore.Mvc;

namespace Chapter03.Errors;

public class CustomHttpProblemDetails : ValidationProblemDetails
{
    public CustomHttpProblemDetails(HttpContext context)
    {
        Title = "Bad Request";
        Status = StatusCodes.Status400BadRequest;
        Detail = "HTTP requests are not allowed. Please use HTTPS.";
        Instance = $"{context.Request.Path} ({context.TraceIdentifier})";

        Dictionary<string, string?> relevantHeaders = new Dictionary<string, string?>
        {
            { "Host", context.Request.Headers["Host"] },
            { "User-Agent", context.Request.Headers["User-Agent"] },
            { "X-Forwarded-Proto", context.Request.Headers["X-Forwarded-Proto"] },
            { "X-Forwarded-For", context.Request.Headers["X-Forwarded-For"] }
        };

        Extensions["headers"] = relevantHeaders;
    }
}
