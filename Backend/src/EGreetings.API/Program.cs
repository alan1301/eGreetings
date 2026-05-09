using EGreetings.API.Middlewares;
using EGreetings.Application;
using EGreetings.Infrastructure;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/egreetings-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ── Services ───────────────────────────────────────────────────────
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// JWT Authentication
var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSection["SecretKey"]!)),
            ClockSkew = TimeSpan.Zero  // No tolerance – strict BR-04
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Allow enums to be deserialized from string names ("Monthly") as well as integers (1)
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:3000"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();    // For httpOnly cookies
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "E-Greetings API",
        Version = "v6.0",
        Description = "Backend API cho hệ thống thiệp điện tử E-Greetings"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── Build App ─────────────────────────────────────────────────────
var app = builder.Build();

// ── Auto-Migrate & Seed ───────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EGreetings.Infrastructure.Persistence.AppDbContext>();
    var seedLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        // SQLite on macOS/Linux → use EnsureCreated (avoids SQL Server type syntax in migrations)
        // SQL Server on Windows/prod → use MigrateAsync for proper versioned migrations
        if (db.Database.ProviderName?.Contains("Sqlite") == true)
            await db.Database.EnsureCreatedAsync();
        else
            await db.Database.MigrateAsync();

        await EGreetings.Infrastructure.Persistence.AppDbContextSeed.SeedAsync(db, seedLogger);
    }
    catch (Exception ex)
    {
        seedLogger.LogError(ex, "[STARTUP] Migration/Seed failed: {Message}", ex.Message);
    }
}

// ── Middleware Pipeline Order ──────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Greetings API v6.0");
        c.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<GlobalExceptionMiddleware>();   // Must be first
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Hangfire Dashboard (Admin only in production)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter()]
});

app.MapControllers();

// Register recurring jobs
app.Services.RegisterHangfireJobs();

app.Run();
