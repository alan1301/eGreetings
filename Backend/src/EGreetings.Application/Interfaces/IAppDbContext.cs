using EGreetings.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Interfaces;

/// <summary>
/// Database context interface – allows Application layer to access DB without coupling to EF Core directly.
/// </summary>
public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Category> Categories { get; }
    DbSet<GreetingCard> GreetingCards { get; }
    DbSet<Draft> Drafts { get; }
    DbSet<GreetingTransaction> GreetingTransactions { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<SubscriptionEmailList> SubscriptionEmailLists { get; }
    DbSet<PaymentTransaction> PaymentTransactions { get; }
    DbSet<Contact> Contacts { get; }
    DbSet<Feedback> Feedbacks { get; }
    DbSet<EmailRetryQueue> EmailRetryQueues { get; }
    DbSet<SystemLog> SystemLogs { get; }
    DbSet<WebsiteContent> WebsiteContents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
