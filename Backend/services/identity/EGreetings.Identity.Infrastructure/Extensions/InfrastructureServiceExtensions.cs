using EGreetings.Identity.Application.Common.Interfaces;
using EGreetings.Identity.Infrastructure.Persistence;
using EGreetings.Identity.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EGreetings.Identity.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("Default");
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("EGreetings.Identity.Infrastructure");
            });
        });

        services.AddScoped<IIdentityDbContext>(provider =>
            provider.GetRequiredService<IdentityDbContext>());

        services.AddSingleton<IJwtService, JwtService>();

        return services;
    }
}
