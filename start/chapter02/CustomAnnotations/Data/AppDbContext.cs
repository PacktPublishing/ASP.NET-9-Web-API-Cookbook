using Microsoft.EntityFrameworkCore;
using CustomAnnotations.Models;

namespace CustomAnnotations.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext>options) : base(options) {}

    public DbSet<EventRegistration> EventRegistrations { get; set; }
}
