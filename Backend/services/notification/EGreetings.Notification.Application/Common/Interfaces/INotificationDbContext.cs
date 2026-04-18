using EGreetings.Notification.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Notification.Application.Common.Interfaces;

public interface INotificationDbContext
{
    DbSet<EmailLog> EmailLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
