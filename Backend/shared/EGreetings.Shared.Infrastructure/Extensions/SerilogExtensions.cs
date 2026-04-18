using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Json;

namespace EGreetings.Shared.Infrastructure.Extensions;

public static class SerilogExtensions
{
    public static IHostBuilder ConfigureSerilog(
        this IHostBuilder hostBuilder,
        string serviceName)
    {
        return hostBuilder.UseSerilog((context, loggerConfiguration) =>
        {
            loggerConfiguration
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ServiceName", serviceName)
                .WriteTo.Console(new JsonFormatter())
                .ReadFrom.Configuration(context.Configuration);
        });
    }
}
