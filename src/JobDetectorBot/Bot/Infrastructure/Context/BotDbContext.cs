using Bot.Domain.DataAccess.Model;
using Microsoft.EntityFrameworkCore;

public class BotDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public BotDbContext(DbContextOptions<BotDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Настройка уникальности UserId
        modelBuilder.Entity<User>()
            .HasIndex(u => u.TelegramId)
            .IsUnique();

        // Настройка связи один-к-одному между User и Criteria
        modelBuilder.Entity<User>()
            .HasOne(u => u.Criteria)
            .WithOne()
            .HasForeignKey<Criteria>(c => c.UserId);
    }
}