namespace Books.GraphQL;

public interface IUserError  
{
    string Message { get; }
    string Code { get; }
}
