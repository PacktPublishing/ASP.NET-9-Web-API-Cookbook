using books.Models;
using books.Services;

namespace books.GraphQL;

public class Query { 
	[UsePaging(IncludeTotalCount = true)]         
	[UseFiltering]         
	[UseSorting]         
	public IQueryable<BookDTO> GetBooks([Service] IBooksService booksService) {             
        return booksService.GetBooks();         
	}     
} 


