using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using ClientFlow.Api.Middleware;
using ClientFlow.Infrastructure;
using ClientFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// 2. CORS Policy for Angular Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientFlowPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200", "http://localhost:5000", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("X-Correlation-ID", "X-Idempotency-Replay");
    });
});

// 3. Rate Limiting Configuration (.NET 8)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("global-limit", opt =>
    {
        opt.PermitLimit = 120;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc9457#section-3",
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Rate Limit Exceeded",
            Detail = "Too many requests. Please retry after some time.",
            Instance = context.HttpContext.Request.Path
        };
        await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(problem), token);
    };
});

// 4. Infrastructure & Application Dependencies
builder.Services.AddInfrastructureServices();

// 5. Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// 6. Seed sample development data
using (var scope = app.Services.CreateScope())
{
    var store = scope.ServiceProvider.GetRequiredService<InMemoryDataStore>();
    SampleDataSeeder.SeedSampleData(store);
}

// 7. HTTP Pipeline
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<IdempotencyMiddleware>();

app.UseCors("ClientFlowPolicy");
app.UseRateLimiter();
app.UseRouting();

app.MapControllers().RequireRateLimiting("global-limit");

// Health Check Endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    application = "ClientFlow API",
    version = "1.1.0"
}));

// Quick API Info & Token Generation for Evaluation
app.MapPost("/api/auth/token", ([FromBody] AuthTokenRequest req) =>
{
    var username = string.IsNullOrWhiteSpace(req.Username) ? "demo_engineer" : req.Username.Trim();
    var role = string.IsNullOrWhiteSpace(req.Role) ? "Admin" : req.Role.Trim();
    return Results.Ok(new
    {
        token = $"demo_jwt_{Guid.NewGuid():N}",
        username,
        role,
        expiresInMinutes = 60,
        tokenType = "Bearer"
    });
});

app.MapGet("/", () => Results.Ok(new
{
    message = "Welcome to ClientFlow Enterprise API",
    documentation = "/api/dashboard/kpis",
    health = "/health"
}));

app.Run();

public record AuthTokenRequest(string? Username, string? Role);
