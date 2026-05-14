using EGreetings.Application.Interfaces;
using EGreetings.Infrastructure.Jobs;
using EGreetings.Infrastructure.Persistence;
using EGreetings.Infrastructure.Services;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace EGreetings.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        // ── EF Core (Code First) — SQL Server only ──────────────
        var connStr = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection chưa được cấu hình. " +
                "Vui lòng set qua appsettings.json hoặc env var ConnectionStrings__DefaultConnection.");

        services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlServer(connStr,
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IAppDbContext>(p => p.GetRequiredService<AppDbContext>());

        // ── Domain Services ──────────────────────────────────────
        services.AddScoped<IPasswordHasher, PasswordHasherService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        // ── Jobs ──────────────────────────────────────────────────
        services.AddScoped<AutoDisableExpiredSubscriptionsJob>();
        services.AddScoped<SendDailyGreetingsJob>();
        services.AddScoped<RetryFailedEmailsJob>();
        services.AddScoped<CleanupOldLogsJob>();

        // ── Hangfire (SQL Server storage) ────────────────────────
        services.AddHangfire(conf =>
        {
            conf.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connStr, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                });
        });

        services.AddHangfireServer();

        return services;
    }

    /// <summary>Register recurring jobs after app starts.</summary>
    public static void RegisterHangfireJobs(this IServiceProvider services)
    {
        // UC15: Auto-disable expired subscriptions at 00:01 daily (BR-16)
        RecurringJob.AddOrUpdate<AutoDisableExpiredSubscriptionsJob>(
            "auto-disable-expired-subscriptions",
            job => job.ExecuteAsync(),
            "1 0 * * *",  // 00:01 daily
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

        // UC20: Send daily greetings at 08:00 (BR-23)
        RecurringJob.AddOrUpdate<SendDailyGreetingsJob>(
            "send-daily-greetings",
            job => job.ExecuteAsync(),
            "0 8 * * *",  // 08:00 daily
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

        // UC29: Retry failed emails every 5 minutes (BR-32)
        RecurringJob.AddOrUpdate<RetryFailedEmailsJob>(
            "retry-failed-emails",
            job => job.ExecuteAsync(),
            "*/5 * * * *",  // every 5 minutes
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

        // UC30: Cleanup old system logs daily (BR-33)
        RecurringJob.AddOrUpdate<CleanupOldLogsJob>(
            "cleanup-old-logs",
            job => job.ExecuteAsync(),
            "0 2 * * *",  // 02:00 daily
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
    }
}
