using Microsoft.EntityFrameworkCore;
using EGreetings.Feedback.Application.Common.Interfaces;
using EGreetings.Feedback.Domain.Entities;

namespace EGreetings.Feedback.Infrastructure;

public class FeedbackDbContext : DbContext, IFeedbackDbContext
{
    public DbSet<global::EGreetings.Feedback.Domain.Entities.Feedback> Feedbacks { get; set; }

    public FeedbackDbContext(DbContextOptions<FeedbackDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Feedback
        modelBuilder.Entity<global::EGreetings.Feedback.Domain.Entities.Feedback>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.ContactEmail).HasMaxLength(255);
            entity.Property(e => e.Reply);
            entity.Property(e => e.Status).HasConversion<int>();

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsRead);
        });
    }
}
