namespace Books.GraphQL;

public class BookAlreadyExistsException   : Exception
{
    public string ISBN { get; }

    public BookAlreadyExistsException(string isbn)
        : base($"A book with ISBN '{isbn}' already exists.")
    {
        ISBN = isbn;
    }
}
