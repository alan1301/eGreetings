using MediatR;
using Microsoft.Extensions.Logging;
using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using System.Text.Json;

namespace EGreetings.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior: log every request entry and exit (UC30).
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly IAuditLogService _auditLogService;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        IAuditLogService auditLogService)
    {
        _logger = logger;
        _auditLogService = auditLogService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var isCommand = requestName.EndsWith("Command");
        var isAdmin = typeof(TRequest).Namespace?.Contains(".Admin.") == true || requestName.Contains("Admin");

        _logger.LogInformation("[CQRS] Handling {RequestName}", requestName);

        try
        {
            var response = await next();
            _logger.LogInformation("[CQRS] Handled {RequestName} successfully", requestName);

            if (isAdmin && isCommand)
            {
                await _auditLogService.LogAsync(
                    EventType.AdminAction,
                    $"Executed {requestName}",
                    LogStatus.Success,
                    actorType: ActorType.Admin,
                    cancellationToken: cancellationToken);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CQRS] Error handling {RequestName}: {Message}", requestName, ex.Message);

            if (isAdmin && isCommand)
            {
                await _auditLogService.LogAsync(
                    EventType.AdminAction,
                    $"Failed to execute {requestName}: {ex.Message}",
                    LogStatus.Failed,
                    actorType: ActorType.Admin,
                    cancellationToken: cancellationToken);
            }

            throw;
        }
    }
}
