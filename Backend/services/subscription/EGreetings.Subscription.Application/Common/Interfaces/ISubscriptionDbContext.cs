using EGreetings.Subscription.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Subscription.Application.Common.Interfaces;

public interface ISubscriptionDbContext
{
    DbSet<global::EGreetings.Subscription.Domain.Entities.Subscription> Subscriptions { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<SubscriptionRecipient> SubscriptionRecipients { get; }
    DbSet<Payment> Payments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
