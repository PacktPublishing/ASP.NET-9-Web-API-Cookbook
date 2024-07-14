namespace books.Middleware;

public class ResponseFormatterMiddlewareFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public ResponseFormatterMiddlewareFactory(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public IResponseFormatterMiddleware Create(HttpContext context)
    {
        var accept = context.Request.Headers["Accept"].FirstOrDefault();
        
        if (accept?.Contains("text/csv") == true)
        {
            return _serviceProvider.GetRequiredService<CsvFormatterMiddleware>();
        }
        else
        {
            return _serviceProvider.GetRequiredService<JsonFormatterMiddleware>();
        }
    }
}
