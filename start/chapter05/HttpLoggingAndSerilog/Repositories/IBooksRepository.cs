using books.Models;
namespace books.Repositories;

public interface IBooksRepository
{
    Task<IReadOnlyCollection<Book>> GetBooksAsync(int pageSize, int lastId);
    Task<Book?> GetBookByIdAsync(int id);
    Task<Book> CreateBookAsync(Book book);
}
