using EGreetings.Notification.Application.Common.Interfaces;
using EGreetings.Notification.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Notification.Infrastructure.Persistence;

public class NotificationDbContext : DbContext, INotificationDbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    public DbSet<EmailLog> EmailLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EmailLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ToEmail)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Subject)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Body)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasConversion<int>();

            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(1000);

            entity.Property(e => e.MessageId)
                .HasMaxLength(255);

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.MessageId).IsUnique();
            entity.HasIndex(e => e.ToEmail);
        });
    }
}
