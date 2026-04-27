using MediatR;
using Microsoft.Extensions.Logging;

namespace EGreetings.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior: log every request entry and exit (UC30).
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("[CQRS] Handling {RequestName}", requestName);

        try
        {
            var response = await next();
            _logger.LogInformation("[CQRS] Handled {RequestName} successfully", requestName);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CQRS] Error handling {RequestName}: {Message}", requestName, ex.Message);
            throw;
        }
    }
}
