namespace EGreetings.Notification.Application.Common.Interfaces;

public interface IIdempotencyRepository
{
    Task<bool> ExistsAsync(string messageId, CancellationToken ct = default);
    Task MarkProcessedAsync(string messageId, CancellationToken ct = default);
}
