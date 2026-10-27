using System.Text.Json.Serialization;
using ClientFlow.Api.Middleware;
using ClientFlow.Infrastructure;
using ClientFlow.Infrastructure.Persistence;

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
              .WithExposedHeaders("X-Correlation-ID");
    });
});

// 3. Infrastructure & Application Dependencies
builder.Services.AddInfrastructureServices();

// 4. Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// 5. Seed synthetic enterprise data for local demonstration
using (var scope = app.Services.CreateScope())
{
    var store = scope.ServiceProvider.GetRequiredService<InMemoryDataStore>();
    SampleDataSeeder.SeedSampleData(store);
}

// 6. HTTP Pipeline
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseCors("ClientFlowPolicy");
app.UseRouting();

app.MapControllers();

// Health Check Endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    application = "ClientFlow API",
    version = "1.0.0"
}));

// Quick API Info
app.MapGet("/", () => Results.Ok(new
{
    message = "Welcome to ClientFlow Enterprise API",
    documentation = "/api/dashboard/kpis",
    health = "/health"
}));

app.Run();
