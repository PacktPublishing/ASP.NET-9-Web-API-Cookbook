using Microsoft.EntityFrameworkCore;
using Books.Data;
using Books.Models;

namespace Books.Repositories;

public class BooksRepository : IBooksRepository
{
    private readonly AppDbContext _context;

    public BooksRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Book>> GetBooksAsync(int pageSize, int lastId)
    {
        return await _context.Books
            .Where(b => b.Id > lastId)
            .OrderBy(b => b.Id)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Book?> GetBookByIdAsync(int id)
    {
        return await _context.Books.FindAsync(id);
    }

    public async Task<Book> CreateBookAsync(Book book)
    {
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<Book?> UpdateBookAsync(int id, Book book)
    {
        var existingBook = await _context.Books.FindAsync(id);
        if (existingBook == null)
        {
            return null;
        }

        existingBook.Title = book.Title;
        existingBook.Author = book.Author;
        existingBook.PublicationDate = book.PublicationDate;
        existingBook.ISBN = book.ISBN;
        existingBook.Genre = book.Genre;
        existingBook.Summary = book.Summary;

        await _context.SaveChangesAsync();
        return existingBook;
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
        {
            return false;
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Book?> GetBookByISBNAsync(string isbn)
    {
            return await _context.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);
    }
}
