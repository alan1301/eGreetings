using EGreetings.Application.Common.Interfaces;
using EGreetings.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Infrastructure.Persistence;

public class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<GreetingTemplate> GreetingTemplates => Set<GreetingTemplate>();
    public DbSet<Greeting> Greetings => Set<Greeting>();
    public DbSet<GreetingTransaction> GreetingTransactions => Set<GreetingTransaction>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionRecipient> SubscriptionRecipients => Set<SubscriptionRecipient>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Draft> Drafts => Set<Draft>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();
    public DbSet<WebContent> WebContents => Set<WebContent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-set UpdatedAt on modified entities
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Modified)
            {
                var prop = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                if (prop != null) prop.CurrentValue = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
