using EGreetings.Domain.Exceptions;
using EGreetings.Shared.Common;
using FluentValidation;
using System.Text.Json;

namespace EGreetings.API.Middlewares;

/// <summary>
/// Global exception handler – maps domain exceptions to HTTP status codes.
/// Prevents leaking stack traces in production.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var statusCode = ex switch
        {
            EntityNotFoundException => StatusCodes.Status404NotFound,
            BusinessRuleViolationException => StatusCodes.Status422UnprocessableEntity,
            ValidationException => StatusCodes.Status400BadRequest,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            ForbiddenException => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        _logger.LogError(ex, "[EXCEPTION] {StatusCode} {Path} – {Message}",
            statusCode, context.Request.Path, ex.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        List<FieldError>? errors = null;
        if (ex is ValidationException valEx)
        {
            errors = valEx.Errors.Select(e => new FieldError
            {
                Field = e.PropertyName,
                Message = e.ErrorMessage
            }).ToList();
        }

        var message = statusCode == 500 && !_env.IsDevelopment()
            ? "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau."
            : ex.Message;

        var response = ApiResponse<object>.Fail(message, errors);

        await context.Response.WriteAsync(JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}

/// <summary>
/// Security headers middleware – XSS, Clickjacking protection.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append(
            "Content-Security-Policy",
            "default-src 'self'; img-src 'self' data: https:; script-src 'self'");
        await _next(context);
    }
}
