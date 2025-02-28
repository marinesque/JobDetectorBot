using Bot;
using Microsoft.EntityFrameworkCore;

public class BotDbContext : DbContext
{
    public DbSet<BotUser> Users { get; set; }

    public BotDbContext(DbContextOptions<BotDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Настройка уникальности UserId
        modelBuilder.Entity<BotUser>()
            .HasIndex(u => u.UserId)
            .IsUnique();
    }
}