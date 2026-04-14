using System.Net;
using System.Text.Json;
using FluentValidation;

namespace EGreetings.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var (statusCode, message, errors) = exception switch
        {
            ValidationException ve => (
                HttpStatusCode.BadRequest,
                "Dữ liệu không hợp lệ.",
                ve.Errors.Select(e => e.ErrorMessage).ToList()
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                exception.Message,
                (List<string>?)null
            ),
            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                exception.Message,
                (List<string>?)null
            ),
            InvalidOperationException => (
                HttpStatusCode.BadRequest,
                exception.Message,
                (List<string>?)null
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "Đã có lỗi xảy ra. Vui lòng thử lại sau.",
                (List<string>?)null
            )
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            Success = false,
            Message = message,
            Errors = errors
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
