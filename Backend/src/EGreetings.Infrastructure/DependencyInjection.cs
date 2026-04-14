using EGreetings.Application.Common.Interfaces;
using EGreetings.Infrastructure.BackgroundServices;
using EGreetings.Infrastructure.Persistence;
using EGreetings.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EGreetings.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // EF Core - SQLite (dễ setup cho dev; đổi sang SQL Server khi deploy)
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=egreetings.db";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<AppDbContext>());

        // Repository + UnitOfWork (project-rules: handlers inject IRepository/IUnitOfWork)
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuditService, AuditService>();

        services.AddHttpContextAccessor();

        // Background Services
        services.AddHostedService<ExpireSubscriptionService>();
        services.AddHostedService<AutoSendGreetingService>();
        services.AddHostedService<RetryEmailService>();
        services.AddHostedService<AuditLogCleanupService>();

        return services;
    }
}
