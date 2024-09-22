using Microsoft.EntityFrameworkCore;
using books.Models;

namespace books.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
    {
    }

    public DbSet<Book> Books { get; set; } = null!;

}
