using Microsoft.EntityFrameworkCore;
using EGreetings.Subscription.Application.Common.Interfaces;
using EGreetings.Subscription.Domain.Entities;

namespace EGreetings.Subscription.Infrastructure;

public class SubscriptionDbContext : DbContext, ISubscriptionDbContext
{
    public DbSet<global::EGreetings.Subscription.Domain.Entities.Subscription> Subscriptions { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<SubscriptionRecipient> SubscriptionRecipients { get; set; }
    public DbSet<Payment> Payments { get; set; }

    public SubscriptionDbContext(DbContextOptions<SubscriptionDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // SubscriptionPlan
        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.PricePerYear).HasPrecision(18, 2);
        });

        // Subscription
        modelBuilder.Entity<global::EGreetings.Subscription.Domain.Entities.Subscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.DisabledReason).HasMaxLength(500);

            entity.HasOne(e => e.Plan)
                .WithMany()
                .HasForeignKey(e => e.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Recipients)
                .WithOne(r => r.Subscription)
                .HasForeignKey(r => r.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Payments)
                .WithOne(p => p.Subscription)
                .HasForeignKey(p => p.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
        });

        // SubscriptionRecipient
        modelBuilder.Entity<SubscriptionRecipient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Occasion).HasMaxLength(255);

            entity.HasIndex(e => new { e.SubscriptionId, e.Email }).IsUnique();
        });

        // Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Method).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TransactionCode).HasMaxLength(255);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
        });
    }
}
