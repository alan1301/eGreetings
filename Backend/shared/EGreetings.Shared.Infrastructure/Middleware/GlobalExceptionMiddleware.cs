using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using EGreetings.Shared.Domain.Exceptions;

namespace EGreetings.Shared.Infrastructure.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            message = string.Empty,
            traceId = context.TraceIdentifier
        };

        switch (exception)
        {
            case NotFoundException notFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = new
                {
                    success = false,
                    message = notFoundEx.Message,
                    traceId = context.TraceIdentifier
                };
                break;

            case ForbiddenException forbiddenEx:
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                response = new
                {
                    success = false,
                    message = forbiddenEx.Message,
                    traceId = context.TraceIdentifier
                };
                break;

            case ConflictException conflictEx:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response = new
                {
                    success = false,
                    message = conflictEx.Message,
                    traceId = context.TraceIdentifier
                };
                break;

            case DomainException domainEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    success = false,
                    message = domainEx.Message,
                    traceId = context.TraceIdentifier
                };
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response = new
                {
                    success = false,
                    message = "An unexpected error occurred",
                    traceId = context.TraceIdentifier
                };
                break;
        }

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsJsonAsync(response, options: options);
    }
}
