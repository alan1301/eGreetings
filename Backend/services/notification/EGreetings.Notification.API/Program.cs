using EGreetings.Notification.Application.Interfaces;
using EGreetings.Notification.Infrastructure.Consumers;
using EGreetings.Notification.Infrastructure.Services;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Tạm thời mock EmailService
builder.Services.AddScoped<IEmailService, EmailService>();

// Config MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<GreetingSentEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        // Trong thực tế sẽ lấy từ builder.Configuration["RabbitMQ:Host"]
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Notification Service" }));

app.Run();
