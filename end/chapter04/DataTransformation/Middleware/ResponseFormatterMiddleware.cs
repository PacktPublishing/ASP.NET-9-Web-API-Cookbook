namespace books.Middleware;

public class ResponseFormatterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ResponseFormatterMiddlewareFactory _factory;
    
    public ResponseFormatterMiddleware(RequestDelegate next, ResponseFormatterMiddlewareFactory factory)
    {
        _next = next;
        _factory = factory;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var formatter = _factory.Create(context);
        context.Response.ContentType = formatter.GetContentType();
        await formatter.InvokeAsync(context, _next);
    }
}
