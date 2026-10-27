using ClientFlow.Domain.Entities;
using ClientFlow.Domain.Enums;

namespace ClientFlow.Domain.Interfaces;

public interface IClientRepository
{
    Task<IReadOnlyList<Client>> GetAllAsync(string? search = null, ClientStatus? status = null, CancellationToken ct = default);
    Task<Client?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Client client, CancellationToken ct = default);
    Task UpdateAsync(Client client, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null, CancellationToken ct = default);
}

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> GetAllAsync(Guid? clientId = null, ProjectStatus? status = null, CancellationToken ct = default);
    Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Project project, CancellationToken ct = default);
    Task UpdateAsync(Project project, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface ITaskRepository
{
    Task<IReadOnlyList<ProjectTask>> GetByProjectIdAsync(Guid projectId, TaskItemStatus? status = null, CancellationToken ct = default);
    Task<ProjectTask?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ProjectTask task, CancellationToken ct = default);
    Task UpdateAsync(ProjectTask task, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface ITimeEntryRepository
{
    Task<IReadOnlyList<TimeEntry>> GetByProjectIdAsync(Guid projectId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    Task<IReadOnlyList<TimeEntry>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default);
    Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(TimeEntry entry, CancellationToken ct = default);
    Task UpdateAsync(TimeEntry entry, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<decimal> GetTotalHoursAsync(Guid projectId, bool? billableOnly = null, CancellationToken ct = default);
}

public interface IInvoiceRepository
{
    Task<IReadOnlyList<Invoice>> GetAllAsync(Guid? clientId = null, InvoiceStatus? status = null, CancellationToken ct = default);
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Invoice?> GetByNumberAsync(string invoiceNumber, CancellationToken ct = default);
    Task AddAsync(Invoice invoice, CancellationToken ct = default);
    Task UpdateAsync(Invoice invoice, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<string> GenerateNextInvoiceNumberAsync(CancellationToken ct = default);
}

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken ct = default);
    Task<IReadOnlyList<AuditLog>> GetRecentAsync(int count = 50, CancellationToken ct = default);
}
