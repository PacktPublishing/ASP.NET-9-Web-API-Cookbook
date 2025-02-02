using Microsoft.EntityFrameworkCore;
using Books.Models;

namespace Books.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
    {
    }

    public DbSet<Book> Books { get; set; } = null!;

}
