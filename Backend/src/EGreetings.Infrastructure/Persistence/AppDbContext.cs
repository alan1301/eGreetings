using EGreetings.Application.Interfaces;
using EGreetings.Domain.Common;
using EGreetings.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    private readonly IMediator? _mediator;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator) : base(options)
    {
        _mediator = mediator;
    }

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
    public DbSet<CardBackground> CardBackgrounds => Set<CardBackground>();
    public DbSet<CardDecoration> CardDecorations => Set<CardDecoration>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Auto-apply all IEntityTypeConfiguration<T> from this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entitiesWithEvents = ChangeTracker.Entries<HasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (_mediator is not null)
        {
            foreach (var entity in entitiesWithEvents)
            {
                var events = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();
                foreach (var ev in events)
                {
                    await _mediator.Publish(ev, cancellationToken);
                }
            }
        }

        return result;
    }
}
