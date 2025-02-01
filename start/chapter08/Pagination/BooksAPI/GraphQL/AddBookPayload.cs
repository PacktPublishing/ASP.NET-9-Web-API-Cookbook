using Books.Models;

namespace books.GraphQL;

public class AddBookPayload
{
	public BookDTO? Book { get; }

	public AddBookPayload(BookDTO book)
	{
		Book = book;
	}
}
