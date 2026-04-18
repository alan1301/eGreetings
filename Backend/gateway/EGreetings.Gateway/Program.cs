using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("ServiceName", "api-gateway")
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting API Gateway");

    // YARP Reverse Proxy
    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
        .AddTransforms(ctx =>
        {
            // Forward UserId, Role, Email from JWT claims as headers to downstream services
            ctx.AddRequestTransform(async reqCtx =>
            {
                var correlationId = reqCtx.HttpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                    ?? Guid.NewGuid().ToString();
                reqCtx.ProxyRequest.Headers.Remove("X-Correlation-ID");
                reqCtx.ProxyRequest.Headers.TryAddWithoutValidation("X-Correlation-ID", correlationId);

                if (reqCtx.HttpContext.User.Identity?.IsAuthenticated == true)
                {
                    var user = reqCtx.HttpContext.User;
                    reqCtx.ProxyRequest.Headers.Remove("X-UserId");
                    reqCtx.ProxyRequest.Headers.Remove("X-UserRole");
                    reqCtx.ProxyRequest.Headers.Remove("X-UserEmail");
                    reqCtx.ProxyRequest.Headers.TryAddWithoutValidation("X-UserId", user.FindFirst("sub")?.Value ?? "");
                    reqCtx.ProxyRequest.Headers.TryAddWithoutValidation("X-UserRole", user.FindFirst("role")?.Value ?? "");
                    reqCtx.ProxyRequest.Headers.TryAddWithoutValidation("X-UserEmail", user.FindFirst("email")?.Value ?? "");
                }
                await Task.CompletedTask;
            });
        });

    // JWT Authentication (validate token at gateway)
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
            };
            // Don't fail immediately - let downstream handle 401
            options.Events = new JwtBearerEvents
            {
                OnChallenge = ctx => { ctx.HandleResponse(); return Task.CompletedTask; }
            };
        });

    builder.Services.AddAuthorization();

    // CORS — allow all Angular frontends (ports 3000, 3001, 3002 and any configured via env)
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? ["http://localhost:3000", "http://localhost:3001", "http://localhost:3002"];

    builder.Services.AddCors(opt =>
    {
        opt.AddDefaultPolicy(p => p
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());

        // Fallback permissive policy for local dev if above doesn't match
        opt.AddPolicy("AllowAll", p =>
            p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    });

    // Rate limiting
    builder.Services.AddRateLimiter(o => o.AddFixedWindowLimiter("default", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
    }));

    // Health checks
    builder.Services.AddHealthChecks();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapReverseProxy();
    app.MapHealthChecks("/health");

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
