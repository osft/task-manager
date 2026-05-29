using System.Net;
using System.Text.Json;
using TaskManager.Application.Exceptions;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
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
        HttpStatusCode statusCode;
        string message;

        switch (exception)
        {
            case ConflictException conflict:
                statusCode = HttpStatusCode.Conflict;
                message = conflict.Message;
                break;
            case UnauthorizedApplicationException unauthorized:
                statusCode = HttpStatusCode.Unauthorized;
                message = unauthorized.Message;
                break;
            case ForbiddenApplicationException forbidden:
                statusCode = HttpStatusCode.Forbidden;
                message = forbidden.Message;
                break;
            case NotFoundException notFound:
                statusCode = HttpStatusCode.NotFound;
                message = notFound.Message;
                break;
            case DomainValidationException domainValidation:
                statusCode = HttpStatusCode.BadRequest;
                message = domainValidation.Message;
                break;
            case ArgumentException argument:
                statusCode = HttpStatusCode.BadRequest;
                message = argument.Message;
                break;
            case Application.Exceptions.ApplicationException application:
                statusCode = HttpStatusCode.BadRequest;
                message = application.Message;
                break;
            default:
                statusCode = HttpStatusCode.InternalServerError;
                message = "An unexpected error occurred.";
                break;
        }

        switch (statusCode)
        {
            case HttpStatusCode.InternalServerError:
                _logger.LogError(
                    exception,
                    "Unhandled exception processing {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
                break;
            case HttpStatusCode.NotFound:
                _logger.LogInformation(
                    "Resource not found: {Method} {Path} — {Message}",
                    context.Request.Method,
                    context.Request.Path,
                    message);
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = JsonSerializer.Serialize(new { error = message });
        await context.Response.WriteAsync(payload);
    }
}
