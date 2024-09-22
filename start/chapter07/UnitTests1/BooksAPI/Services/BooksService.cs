using books.Repositories;
using books.Models;
using Microsoft.AspNetCore.Mvc;

namespace books.Services;

public class BooksService : IBooksService
{
    private readonly IBooksRepository _repository;

    public BooksService(IBooksRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<BookDTO>> GetBooksAsync(int pageSize, int lastId, IUrlHelper urlHelper)
    {
        var books = await _repository.GetBooksAsync(pageSize + 1, lastId);
        
        var items = books.Take(pageSize).Select(b => new BookDTO
        {
            Id = b.Id,
            Title = b.Title,
            Author = b.Author,
            PublicationDate = b.PublicationDate,
            ISBN = b.ISBN,
            Genre = b.Genre,
            Summary = b.Summary
        }).ToList().AsReadOnly();

        var hasPreviousPage = lastId > 0;
        var hasNextPage = books.Count > pageSize;

        return new PagedResult<BookDTO>
        {
            Items = items,
            HasPreviousPage = hasPreviousPage,
            HasNextPage = hasNextPage,
            PreviousPageUrl = hasPreviousPage && items.Any() 
                ? urlHelper.Action("GetBooks", new { pageSize, lastId = lastId - pageSize }) 
                : null,
            NextPageUrl = hasNextPage 
                ? urlHelper.Action("GetBooks", new { pageSize, lastId = items.Last().Id }) 
                : null,
            PageSize = pageSize
        };
    }

    public async Task<BookDTO?> GetBookByIdAsync(int id)
    {
        var book = await _repository.GetBookByIdAsync(id);
        if (book == null) return null;
        return new BookDTO
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            PublicationDate = book.PublicationDate,
            ISBN = book.ISBN,
            Genre = book.Genre,
            Summary = book.Summary
        };
    }

    public async Task<BookDTO> CreateBookAsync(BookDTO bookDTO) 
    {
        var book = new Book
        {
            Title = bookDTO.Title,
            Author = bookDTO.Author,
            PublicationDate = bookDTO.PublicationDate,
            ISBN = bookDTO.ISBN,
            Genre = bookDTO.Genre,
            Summary = bookDTO.Summary
        };
        var result = await _repository.CreateBookAsync(book);
        return new BookDTO
        {
            Id = result.Id,
            Title = result.Title,
            Author = result.Author,
            PublicationDate = result.PublicationDate,
            ISBN = result.ISBN,
            Genre = result.Genre,
            Summary = result.Summary
        };
    }
}
