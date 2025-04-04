using Books.Data;
using Books.Models;

namespace Tests.Integration;

public static class Utilities
{
    public static void InitializeDatabase(AppDbContext context)
    {
        // Clear existing data
        context.Books.RemoveRange(context.Books);
        context.SaveChanges();

        context.Books.AddRange(
            new Book { Title = "Test Book 1", Author = "Author 1", ISBN = "1234567890" },
            new Book { Title = "Test Book 2", Author = "Author 2", ISBN = "0987654321" }
        );

        context.SaveChanges();
    }
}
