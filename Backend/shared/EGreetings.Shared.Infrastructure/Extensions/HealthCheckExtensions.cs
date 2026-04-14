using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EGreetings.Shared.Infrastructure.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddServiceHealthChecks(
        this IServiceCollection services, IConfiguration config)
    {
        var builder = services.AddHealthChecks();

        // Tùy từng service sẽ có thêm DB check, RabbitMQ check, Redis check
        // Ở đây chỉ cung cấp base method cho các service gọi
        return services;
    }
}
