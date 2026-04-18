using EGreetings.Shared.Infrastructure.Middleware;
using EGreetings.Shared.Infrastructure.Extensions;
using EGreetings.Notification.Application.Common.Interfaces;
using EGreetings.Notification.Infrastructure.BackgroundServices;
using EGreetings.Notification.Infrastructure.Messaging.Consumers;
using EGreetings.Notification.Infrastructure.Persistence;
using EGreetings.Notification.Infrastructure.Services;
using EGreetings.Shared.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("ServiceName", "notification-service")
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting Notification Service");

    // Add services
    builder.Services.AddScoped<INotificationDbContext>(provider =>
        provider.GetRequiredService<NotificationDbContext>());

    builder.Services.AddDbContext<NotificationDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("Default");
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly("EGreetings.Notification.Infrastructure");
        });
        options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    });

    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();

    // MassTransit with RabbitMQ
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<UserRegisteredEventConsumer>();
        x.AddConsumer<GreetingSentEventConsumer>();
        x.AddConsumer<PaymentConfirmedEventConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
            var rabbitMqConfig = builder.Configuration.GetSection("RabbitMQ");
            cfg.Host(rabbitMqConfig["Host"], h =>
            {
                h.Username(rabbitMqConfig["Username"]!);
                h.Password(rabbitMqConfig["Password"]!);
            });

            cfg.ReceiveEndpoint("user-registered-queue", e => e.ConfigureConsumer<UserRegisteredEventConsumer>(context));
            cfg.ReceiveEndpoint("greeting-sent-queue", e => e.ConfigureConsumer<GreetingSentEventConsumer>(context));
            cfg.ReceiveEndpoint("payment-confirmed-queue", e => e.ConfigureConsumer<PaymentConfirmedEventConsumer>(context));

            cfg.ConfigureEndpoints(context);
        });
    });

    // Background services
    builder.Services.AddHostedService<RetryEmailService>();

    // Health checks
    builder.Services.AddServiceHealthChecks(builder.Configuration);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Middlewares
    app.UseMiddleware<CorrelationIdMiddleware>("notification-service");
    app.UseMiddleware<GlobalExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false
    });
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });

    // Database initialization
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        Log.Information("Database initialization completed");
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
