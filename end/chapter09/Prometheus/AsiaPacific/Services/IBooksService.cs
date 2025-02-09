using Books.Models;
using Microsoft.AspNetCore.Mvc;

namespace Books.Services;

public interface IBooksService
{
    Task<PagedResult<BookDTO>> GetBooksAsync(int pageSize, int lastId, IUrlHelper urlHelper);
    Task<BookDTO?> GetBookByIdAsync(int id);
    Task<BookDTO> CreateBookAsync(BookDTO bookDTO);
    Task<bool> BookExistsAsync(string isbn);
    IQueryable<BookDTO> GetBooks();
}
