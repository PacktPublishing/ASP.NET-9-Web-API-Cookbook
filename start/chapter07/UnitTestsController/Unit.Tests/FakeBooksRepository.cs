using books.Repositories;
using books.Models;

namespace Tests.Services;

public class FakeBooksRepository : IBooksRepository
{
    private readonly Book _bookToReturn;

    public FakeBooksRepository(Book bookToReturn)
    {
        _bookToReturn = bookToReturn;
    }

    public Task<Book?> GetBookByIdAsync(int id)
    {

     if (_bookToReturn != null && _bookToReturn.Id == id)
       {
          return Task.FromResult<Book?>(_bookToReturn);
       }

          return Task.FromResult<Book?>(null);
    }

    public Task<IReadOnlyCollection<Book>> GetBooksAsync(int pageSize, int lastId)
    {
        throw new NotImplementedException("GetBooksAsync is not implemented in FakeBooksRepository.");

    }

    public Task<Book> CreateBookAsync(Book book)
    {
       throw new NotImplementedException("CreateBookAsync is not implemented in FakeBooksREpository.");
    }
}
