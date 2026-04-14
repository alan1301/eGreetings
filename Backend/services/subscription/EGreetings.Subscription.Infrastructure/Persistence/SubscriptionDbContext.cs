using EGreetings.Subscription.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Subscription.Infrastructure.Persistence;

public class SubscriptionDbContext : DbContext
{
    public SubscriptionDbContext(DbContextOptions<SubscriptionDbContext> options) : base(options) { }

    public DbSet<SubscriptionPlan> Subscriptions => Set<SubscriptionPlan>();
    public DbSet<SubscriptionRecipient> Recipients => Set<SubscriptionRecipient>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscriptionPlan>()
            .HasMany(s => s.Recipients)
            .WithOne()
            .HasForeignKey(r => r.SubscriptionId);
    }
}
