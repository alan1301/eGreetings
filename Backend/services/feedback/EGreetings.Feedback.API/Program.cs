using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using EGreetings.Shared.Infrastructure.Extensions;
using EGreetings.Shared.Infrastructure.Middleware;
using EGreetings.Feedback.Application.Common.Interfaces;
using EGreetings.Feedback.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<FeedbackDbContext>(options =>
    options.UseSqlServer(connectionString, o => o.MigrationsAssembly("EGreetings.Feedback.Infrastructure"))
           .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddScoped<IFeedbackDbContext>(provider => provider.GetRequiredService<FeedbackDbContext>());

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssembly(AppDomain.CurrentDomain.GetAssemblies()
        .FirstOrDefault(a => a.GetName().Name == "EGreetings.Feedback.Application"));
});

// Authentication
builder.Services.AddAuthentication("Bearer")
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, SimpleAuthenticationHandler>("Bearer", null);

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});
builder.Services.AddServiceHealthChecks(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<CorrelationIdMiddleware>("feedback-service");
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("AllowAll");
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
    var context = scope.ServiceProvider.GetRequiredService<FeedbackDbContext>();
    context.Database.EnsureCreated();
}

app.Run();

// Simple Bearer Token Authentication Handler
public class SimpleAuthenticationHandler : Microsoft.AspNetCore.Authentication.AuthenticationHandler<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions>
{
    public SimpleAuthenticationHandler(
        Microsoft.Extensions.Options.IOptionsMonitor<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions> options,
        Microsoft.Extensions.Logging.ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<Microsoft.AspNetCore.Authentication.AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Context.Request.Headers["X-UserId"].ToString();
        var userRole = Context.Request.Headers["X-UserRole"].ToString();

        var claims = new[] {
            new System.Security.Claims.Claim("sub", userId ?? "anonymous"),
            new System.Security.Claims.Claim("role", userRole ?? "Guest")
        };

        var identity = new System.Security.Claims.ClaimsIdentity(claims, Scheme.Name);
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        var ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(Microsoft.AspNetCore.Authentication.AuthenticateResult.Success(ticket));
    }
}
