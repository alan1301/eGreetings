using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace EGreetings.Shared.Infrastructure.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _serviceName;

    public CorrelationIdMiddleware(RequestDelegate next, string serviceName = "unknown")
    {
        _next = next;
        _serviceName = serviceName;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("ServiceName", _serviceName))
        {
            await _next(context);
        }
    }
}
