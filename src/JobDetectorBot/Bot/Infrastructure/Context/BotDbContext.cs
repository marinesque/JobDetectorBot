using Bot.Domain.DataAccess.Model;
using Microsoft.EntityFrameworkCore;

public class BotDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<CriteriaStep> CriteriaSteps { get; set; }
    public DbSet<CriteriaStepValue> CriteriaStepValues { get; set; }
    public DbSet<UserCriteriaStepValue> UserCriteriaStepValues { get; set; }

    public BotDbContext(DbContextOptions<BotDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.TelegramId)
            .IsUnique();

        modelBuilder.Entity<CriteriaStep>()
            .HasIndex(cs => cs.Name)
            .IsUnique();

        modelBuilder.Entity<CriteriaStep>()
            .HasMany(cs => cs.CriteriaStepValues)
            .WithOne(csv => csv.CriteriaStep)
            .HasForeignKey(csv => csv.CriteriaStepId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CriteriaStepValue>()
            .HasIndex(csv => new { csv.CriteriaStepId, csv.Value })
            .IsUnique();

        modelBuilder.Entity<UserCriteriaStepValue>()
            .HasKey(ucsv => ucsv.Id);

        modelBuilder.Entity<UserCriteriaStepValue>()
            .HasOne(ucsv => ucsv.User)
            .WithMany(u => u.UserCriteriaStepValues)
            .HasForeignKey(ucsv => ucsv.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserCriteriaStepValue>()
            .HasOne(ucsv => ucsv.CriteriaStep)
            .WithMany()
            .HasForeignKey(ucsv => ucsv.CriteriaStepId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserCriteriaStepValue>()
            .HasOne(ucsv => ucsv.CriteriaStepValue)
            .WithMany()
            .HasForeignKey(ucsv => ucsv.CriteriaStepValueId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserCriteriaStepValue>()
            .HasIndex(ucsv => new { ucsv.UserId, ucsv.CriteriaStepId })
            .IsUnique(false);

        modelBuilder.Entity<UserCriteriaStepValue>()
            .Property(ucsv => ucsv.CreatedAt)
            .HasDefaultValueSql("now()");

        modelBuilder.Entity<UserCriteriaStepValue>()
            .Property(ucsv => ucsv.UpdatedAt)
            .HasDefaultValueSql("now()");
    }
}