using EGreetings.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Category> Categories { get; }
    DbSet<GreetingTemplate> GreetingTemplates { get; }
    DbSet<Greeting> Greetings { get; }
    DbSet<GreetingTransaction> GreetingTransactions { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<SubscriptionRecipient> SubscriptionRecipients { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Contact> Contacts { get; }
    DbSet<Draft> Drafts { get; }
    DbSet<Feedback> Feedbacks { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<EmailLog> EmailLogs { get; }
    DbSet<WebContent> WebContents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
