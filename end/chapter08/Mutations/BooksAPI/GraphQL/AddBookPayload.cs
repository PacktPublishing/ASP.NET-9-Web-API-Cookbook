using Books.Models;

namespace Books.GraphQL;

public class AddBookPayload
{
    public BookDTO? Book { get; }

    public AddBookPayload(BookDTO book)
    {
         Book = book;
    }
}
