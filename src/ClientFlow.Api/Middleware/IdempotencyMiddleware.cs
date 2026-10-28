using System.Text.Json;
using ClientFlow.Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClientFlow.Api.Middleware;

public class IdempotencyMiddleware
{
    private const string IdempotencyHeader = "Idempotency-Key";
    private const string ReplayHeader = "X-Idempotency-Replay";
    private readonly RequestDelegate _next;

    public IdempotencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IIdempotencyStore idempotencyStore)
    {
        // Only enforce for mutating HTTP methods
        if (!HttpMethods.IsPost(context.Request.Method) &&
            !HttpMethods.IsPut(context.Request.Method) &&
            !HttpMethods.IsPatch(context.Request.Method))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(IdempotencyHeader, out var rawKey) || string.IsNullOrWhiteSpace(rawKey))
        {
            await _next(context);
            return;
        }

        var key = rawKey.ToString().Trim();
        if (key.Length > 128)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";
            var badRequest = new ProblemDetails
            {
                Type = "https://datatracker.ietf.org/doc/html/rfc9457#section-3",
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Idempotency Key",
                Detail = "Idempotency-Key header length must not exceed 128 characters.",
                Instance = context.Request.Path
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(badRequest));
            return;
        }

        // Check cache
        var cached = await idempotencyStore.GetAsync(key);
        if (cached != null)
        {
            context.Response.StatusCode = cached.StatusCode;
            context.Response.ContentType = cached.ContentType;
            context.Response.Headers[ReplayHeader] = "true";
            await context.Response.WriteAsync(cached.Body);
            return;
        }

        // Acquire in-flight lock to prevent race conditions
        bool lockAcquired = await idempotencyStore.TryLockAsync(key, TimeSpan.FromSeconds(30));
        if (!lockAcquired)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/problem+json";
            var conflict = new ProblemDetails
            {
                Type = "https://datatracker.ietf.org/doc/html/rfc9457#section-3",
                Status = StatusCodes.Status409Conflict,
                Title = "Concurrent Request Conflict",
                Detail = "A concurrent request with the specified Idempotency-Key is currently being processed.",
                Instance = context.Request.Path
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(conflict));
            return;
        }

        var originalBodyStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        try
        {
            await _next(context);

            memoryStream.Position = 0;
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

            // Cache successful mutations (2xx)
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                var contentType = context.Response.ContentType ?? "application/json";
                await idempotencyStore.SetAsync(key, context.Response.StatusCode, contentType, responseBody, TimeSpan.FromHours(24));
                context.Response.Headers[ReplayHeader] = "false";
            }

            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
            await idempotencyStore.ReleaseLockAsync(key);
        }
    }
}
