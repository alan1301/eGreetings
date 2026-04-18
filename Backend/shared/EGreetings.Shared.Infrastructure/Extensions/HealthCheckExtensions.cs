using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EGreetings.Shared.Infrastructure.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddServiceHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration,
        string dbConnectionStringKey = "ConnectionStrings:Default")
    {
        var connectionString = configuration[dbConnectionStringKey]
                            ?? configuration.GetConnectionString("Default");

        var checks = services.AddHealthChecks();

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            checks.AddSqlServer(connectionString, name: "database", tags: ["ready"]);
        }

        return services;
    }
}
