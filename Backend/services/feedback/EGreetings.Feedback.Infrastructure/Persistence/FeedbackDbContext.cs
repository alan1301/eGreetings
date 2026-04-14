using EGreetings.Feedback.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Feedback.Infrastructure.Persistence;

public class FeedbackDbContext : DbContext
{
    public FeedbackDbContext(DbContextOptions<FeedbackDbContext> options) : base(options) { }

    public DbSet<FeedbackEntry> Feedbacks => Set<FeedbackEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FeedbackEntry>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Content).IsRequired().HasMaxLength(2000);
        });
    }
}
