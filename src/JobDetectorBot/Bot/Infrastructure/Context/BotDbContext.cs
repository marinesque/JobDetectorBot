using Microsoft.EntityFrameworkCore;

namespace Bot.Domain.DataAccess.Model
{
    public class BotDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Criteria> Criteria { get; set; }
        public DbSet<CriteriaStep> CriteriaSteps { get; set; }
        public DbSet<CriteriaStepValue> CriteriaStepValues { get; set; }

        public BotDbContext(DbContextOptions<BotDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.TelegramId)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Criteria)
                .WithOne()
                .HasForeignKey<Criteria>(c => c.UserId);

            modelBuilder.Entity<CriteriaStep>()
                .HasIndex(cs => cs.Name)
                .IsUnique(); // Уникальный индекс на поле Name

            modelBuilder.Entity<CriteriaStep>()
                .HasMany(cs => cs.CriteriaStepValues)
                .WithOne(csv => csv.CriteriaStep)
                .HasForeignKey(csv => csv.CriteriaStepId);

            modelBuilder.Entity<CriteriaStepValue>()
                .HasIndex(csv => new { csv.CriteriaStepId, csv.Value })
                .IsUnique();
        }
    }
}