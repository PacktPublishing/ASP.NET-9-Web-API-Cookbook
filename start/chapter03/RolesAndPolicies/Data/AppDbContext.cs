using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using events.Models;

namespace events.Data;

public class AppDbContext : IdentityDbContext 
{
    public AppDbContext(DbContextOptions<AppDbContext>options) : base(options) {}

    public DbSet<EventRegistration> EventRegistrations { get; set; } = null!;
}
