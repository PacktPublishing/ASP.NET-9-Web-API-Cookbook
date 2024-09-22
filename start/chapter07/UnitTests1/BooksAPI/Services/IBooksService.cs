using books.Models;
using Microsoft.AspNetCore.Mvc;

namespace books.Services;

public interface IBooksService
{
    Task<PagedResult<BookDTO>> GetBooksAsync(int pageSize, int lastId, IUrlHelper urlHelper);
    Task<BookDTO?> GetBookByIdAsync(int id);
    Task<BookDTO> CreateBookAsync(BookDTO bookDTO);
}
