using EGreetings.Shared.Contracts.Events.Identity;
using EGreetings.Shared.Infrastructure.Middleware;
using EGreetings.User.Domain.Entities;
using EGreetings.User.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<UserDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=egreetings_user.db"));

// Consume UserRegisteredEvent để tạo local user profile
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserRegisteredEventConsumer>();
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
        cfg.ConfigureEndpoints(ctx);
    });
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.UseMiddleware<CorrelationIdMiddleware>("user-service");
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "User Service", Port = 5004 }));

app.Run();

// Consumer: tạo local UserProfile khi Identity Service đăng ký user mới
public class UserRegisteredEventConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly UserDbContext _db;
    public UserRegisteredEventConsumer(UserDbContext db) { _db = db; }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var exists = await _db.UserProfiles.AnyAsync(u => u.IdentityUserId == context.Message.UserId);
        if (exists) return;

        _db.UserProfiles.Add(new UserProfile
        {
            IdentityUserId = context.Message.UserId,
            FullName = context.Message.FullName,
            Email = context.Message.Email,
            CreatedAt = context.Message.RegisteredAt
        });
        await _db.SaveChangesAsync(context.CancellationToken);
    }
}
