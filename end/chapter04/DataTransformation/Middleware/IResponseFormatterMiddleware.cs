namespace books.Middleware;

public interface IResponseFormatterMiddleware : IMiddleware
{
    string GetContentType();
}
