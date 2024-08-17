using Microsoft.EntityFrameworkCore;

public class VoteDbContext : DbContext
{
    public VoteDbContext(DbContextOptions<VoteDbContext> options) : base(options) { }

    public DbSet<Vote> Votes =>Set<Vote>();
}
