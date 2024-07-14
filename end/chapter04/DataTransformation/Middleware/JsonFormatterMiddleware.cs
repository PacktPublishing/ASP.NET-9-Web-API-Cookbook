using System.Text;

namespace books.Middleware;

public class JsonFormatterMiddleware : IResponseFormatterMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var originalBodyStream = context.Response.Body;
        
        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            await responseBody.WriteAsync(Encoding.UTF8.GetBytes("JSON Formatter Middleware Incoming Request \n"));
            
            await next(context);

            if (context.Response.StatusCode == 200)
            {
                await responseBody.WriteAsync(Encoding.UTF8.GetBytes("Response formatted as JSON \n"));
            }
            
            await responseBody.WriteAsync(Encoding.UTF8.GetBytes("JSON Formatter Middleware Outgoing Response \n"));

            responseBody.Seek(0, SeekOrigin.Begin);
            
            context.Response.ContentLength = responseBody.Length;
            
            await responseBody.CopyToAsync(originalBodyStream);
        }
        
        context.Response.Body = originalBodyStream;
    }

    public string GetContentType() => "application/json";
}
