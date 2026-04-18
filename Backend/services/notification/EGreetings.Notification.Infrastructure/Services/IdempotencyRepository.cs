using EGreetings.Notification.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace EGreetings.Notification.Infrastructure.Services;

public class IdempotencyRepository : IIdempotencyRepository
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(24);

    public IdempotencyRepository(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<bool> ExistsAsync(string messageId, CancellationToken ct = default)
    {
        return await Task.FromResult(_cache.TryGetValue($"idempotency_{messageId}", out _));
    }

    public async Task MarkProcessedAsync(string messageId, CancellationToken ct = default)
    {
        var cacheKey = $"idempotency_{messageId}";
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(_cacheDuration);

        _cache.Set(cacheKey, true, cacheOptions);
        await Task.CompletedTask;
    }
}
