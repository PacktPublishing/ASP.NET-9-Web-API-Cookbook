using Microsoft.EntityFrameworkCore;
using events.Models;

namespace events.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext>options) : base(options) {}

    public DbSet<EventRegistration> EventRegistrations { get; set; } = null!;
}
