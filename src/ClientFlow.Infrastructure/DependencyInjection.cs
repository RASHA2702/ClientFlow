using ClientFlow.Application.Services;
using ClientFlow.Domain.Interfaces;
using ClientFlow.Infrastructure.Export;
using ClientFlow.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace ClientFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Singleton in-memory datastore with synthetic enterprise seed data
        services.AddSingleton<InMemoryDataStore>();

        // Repositories
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Application Services
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ITimeTrackingService, TimeTrackingService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IDashboardService, DashboardService>();

        // Exporter
        services.AddSingleton<ICsvExporter, CsvExporter>();

        return services;
    }
}
