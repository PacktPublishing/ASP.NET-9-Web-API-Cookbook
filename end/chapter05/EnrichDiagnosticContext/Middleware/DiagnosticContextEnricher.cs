using Serilog;

namespace Books.Middleware;

public class DiagnosticContextEnricher
{
    public void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext) 
    {
        ArgumentNullException.ThrowIfNull(diagnosticContext);
        ArgumentNullException.ThrowIfNull(httpContext);

        var request = httpContext.Request;

        diagnosticContext.Set("QueryParameters", request.QueryString.Value ?? "");
        diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        if (httpContext.GetEndpoint() is {} endpoint)
        {
            diagnosticContext.Set("EndpointName", endpoint.DisplayName ?? "");
        }
        diagnosticContext.Set("ContentType", httpContext.Response.ContentType ?? "none");

        var isCached = httpContext.Response.Headers.ContainsKey("X-Cache");
        diagnosticContext.Set("IsCached", isCached);
    }
}
