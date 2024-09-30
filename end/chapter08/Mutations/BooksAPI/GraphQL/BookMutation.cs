using books.Models;
using books.Services;
using HotChocolate;
using HotChocolate.Subscriptions;
using HotChocolate.Types;

namespace books.GraphQL;

[ExtendObjectType(Name = "Mutation")]
public class BookMutations
{
	[Error(typeof(BookAlreadyExistsException))]
	public async Task<AddBookPayload> AddBookAsync(
		AddBookInput input,
		[Service] IBooksService booksService,
		[Service] ITopicEventSender eventSender)
	{

    if (await booksService.BookExistsAsync(input.ISBN))
    {
        throw new BookAlreadyExistsException(input.ISBN);
    }

    var bookDto = new BookDTO
    {
        Title = input.Title,
        Author = input.Author,
        PublicationDate = input.PublicationDate,
        ISBN = input.ISBN,
        Genre = input.Genre,
        Summary = input.Summary ?? string.Empty
    };

    var createdBook = await booksService.CreateBookAsync(bookDto);

    await eventSender.SendAsync(nameof(Subscription.OnNewBookAdded), createdBook);

        return new AddBookPayload(createdBook);
    }

}
