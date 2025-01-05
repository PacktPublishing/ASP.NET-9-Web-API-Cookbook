namespace Books.Middleware;

public interface IResponseFormatterMiddleware : IMiddleware
{
    string GetContentType();
}
