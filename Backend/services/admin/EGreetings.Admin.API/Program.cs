using EGreetings.Shared.Infrastructure.Middleware;
using EGreetings.Shared.Infrastructure.Extensions;
using EGreetings.Admin.Application.Common.Interfaces;
using EGreetings.Admin.Infrastructure.Messaging.Consumers;
using EGreetings.Admin.Infrastructure.Persistence;
using EGreetings.Admin.Infrastructure.Services;
using EGreetings.Shared.Infrastructure;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("ServiceName", "admin-service")
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting Admin Service");

    // Add services
    builder.Services.AddScoped<IAdminDbContext>(provider =>
        provider.GetRequiredService<AdminDbContext>());

    builder.Services.AddDbContext<AdminDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("Default");
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly("EGreetings.Admin.Infrastructure");
        });
        options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    });

    // MediatR
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(Assembly.Load("EGreetings.Admin.Application"));
    });

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

            cfg.ReceiveEndpoint("user-registered-admin-queue", e => e.ConfigureConsumer<UserRegisteredEventConsumer>(context));
            cfg.ReceiveEndpoint("greeting-sent-admin-queue", e => e.ConfigureConsumer<GreetingSentEventConsumer>(context));
            cfg.ReceiveEndpoint("payment-confirmed-admin-queue", e => e.ConfigureConsumer<PaymentConfirmedEventConsumer>(context));

            cfg.ConfigureEndpoints(context);
        });
    });

    // HTTP clients for internal services
    builder.Services.AddHttpClient("IdentityService", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["InternalServices:IdentityService"] ?? "http://localhost:5001");
    });

    builder.Services.AddHttpClient("SubscriptionService", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["InternalServices:SubscriptionService"] ?? "http://localhost:5003");
    });

    builder.Services.AddHttpClient("GreetingService", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["InternalServices:GreetingService"] ?? "http://localhost:5002");
    });

    builder.Services.AddHttpClient("FeedbackService", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["InternalServices:FeedbackService"] ?? "http://localhost:5004");
    });

    // Background services
    builder.Services.AddHostedService<AuditLogCleanupService>();

    // Health checks
    builder.Services.AddServiceHealthChecks(builder.Configuration);

    // Authentication (read X-UserRole headers set by gateway)
    builder.Services.AddAuthentication()
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, HeaderAuthenticationHandler>("HeaderAuth", null);

    builder.Services.AddAuthorization();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Middlewares
    app.UseMiddleware<CorrelationIdMiddleware>("admin-service");
    app.UseMiddleware<GlobalExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
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
        var dbContext = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
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

// Simple header-based authentication for internal services
public class HeaderAuthenticationHandler : Microsoft.AspNetCore.Authentication.AuthenticationHandler<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions>
{
    public HeaderAuthenticationHandler(Microsoft.Extensions.Options.IOptionsMonitor<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions> options, Microsoft.Extensions.Logging.ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override async Task<Microsoft.AspNetCore.Authentication.AuthenticateResult> HandleAuthenticateAsync()
    {
        var userIdHeader = Request.Headers["X-UserId"].ToString();
        var userRoleHeader = Request.Headers["X-UserRole"].ToString();
        var userEmailHeader = Request.Headers["X-UserEmail"].ToString();

        if (string.IsNullOrEmpty(userIdHeader))
        {
            return Microsoft.AspNetCore.Authentication.AuthenticateResult.NoResult();
        }

        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim("sub", userIdHeader),
            new System.Security.Claims.Claim("role", userRoleHeader ?? ""),
            new System.Security.Claims.Claim("email", userEmailHeader ?? "")
        };

        var identity = new System.Security.Claims.ClaimsIdentity(claims, Scheme.Name);
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        var ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(principal, Scheme.Name);

        return Microsoft.AspNetCore.Authentication.AuthenticateResult.Success(ticket);
    }
}
