using books.Data;
using books.Models;

namespace Tests.Integration;

public static class Utilities
{
    public static void InitializeDatabase(AppDbContext context)
    {
        // Clear existing data
        context.Books.RemoveRange(context.Books);
        context.SaveChanges();

        // Add seed data
        context.Books.AddRange(
            new Book { Title = "Test Book 1", Author = "Author 1", ISBN = "1234567890" },
            new Book { Title = "Test Book 2", Author = "Author 2", ISBN = "0987654321" }
            // Add more test books as needed
        );

		// Save changes
        context.SaveChanges();
    }
}
