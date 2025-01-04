using Microsoft.EntityFrameworkCore;
using FluentExample.Models;

namespace FluentExample.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext>options) : base(options) {}

    public DbSet<EventRegistration>? EventRegistrations { get; set; }
}
