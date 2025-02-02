using Books.Models;
using Books.Services;

namespace Books.GraphQL;

public class Query
{
    [UsePaging(IncludeTotalCount = true)]
    [UseFiltering]
    public IQueryable<BookDTO> GetBooks([Service] IBooksService booksService) {
        return booksService.GetBooks();
    }
} 
