using EGreetings.Application.Interfaces;
using EGreetings.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<GreetingCard> GreetingCards => Set<GreetingCard>();
    public DbSet<Draft> Drafts => Set<Draft>();
    public DbSet<GreetingTransaction> GreetingTransactions => Set<GreetingTransaction>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionEmailList> SubscriptionEmailLists => Set<SubscriptionEmailList>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<EmailRetryQueue> EmailRetryQueues => Set<EmailRetryQueue>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();
    public DbSet<WebsiteContent> WebsiteContents => Set<WebsiteContent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Auto-apply all IEntityTypeConfiguration<T> from this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
