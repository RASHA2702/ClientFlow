using System.Text.Json;
using ClientFlow.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ClientFlow.Api.Middleware;

public class CorrelationIdMiddleware
{
    private const string CorrelationHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationHeader, out var correlationId) || string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        context.Response.Headers[CorrelationHeader] = correlationId;
        context.Items[CorrelationHeader] = correlationId.ToString();

        await _next(context);
    }
}

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
        var correlationId = context.Items["X-Correlation-ID"]?.ToString() ?? "N/A";
        int statusCode;
        string title;
        string detail = exception.Message;

        switch (exception)
        {
            case EntityNotFoundException notFound:
                statusCode = StatusCodes.Status404NotFound;
                title = "Resource Not Found";
                _logger.LogWarning(exception, "[Correlation: {CorrelationId}] Resource not found: {Message}", correlationId, notFound.Message);
                break;

            case BusinessRuleValidationException validation:
                statusCode = StatusCodes.Status400BadRequest;
                title = "Business Rule Validation Failed";
                _logger.LogWarning(exception, "[Correlation: {CorrelationId}] Validation error for '{Property}': {Message}", correlationId, validation.PropertyName, validation.Message);
                break;

            case DomainException domain:
                statusCode = StatusCodes.Status400BadRequest;
                title = "Domain Error";
                _logger.LogWarning(exception, "[Correlation: {CorrelationId}] Domain error: {Message}", correlationId, domain.Message);
                break;

            default:
                statusCode = StatusCodes.Status500InternalServerError;
                title = "An unexpected server error occurred";
                detail = "An internal error occurred. Please reference the correlation ID when reporting this issue.";
                _logger.LogError(exception, "[Correlation: {CorrelationId}] Unhandled exception: {Message}", correlationId, exception.Message);
                break;
        }

        var problem = new ProblemDetails
        {
            Type = $"https://datatracker.ietf.org/doc/html/rfc9457#section-3",
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };
        problem.Extensions["correlationId"] = correlationId;

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
