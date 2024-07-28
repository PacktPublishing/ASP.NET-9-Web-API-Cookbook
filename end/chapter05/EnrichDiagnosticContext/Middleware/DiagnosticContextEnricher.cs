using Serilog;

namespace books.Middleware;

public static class DiagnosticContextEnricher
{
    public static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
    {
        var request = httpContext.Request;
        
        diagnosticContext.Set("QueryParameters", request.QueryString.Value);
        diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        
        var endpoint = httpContext.GetEndpoint();
        if (endpoint != null)
        {
            diagnosticContext.Set("EndpointName", endpoint.DisplayName);
        }
        
        diagnosticContext.Set("ContentType", httpContext.Response.ContentType);
        var isCached = httpContext.Response.Headers.ContainsKey("X-Cache");
        diagnosticContext.Set("IsCached", isCached);

    }
}
