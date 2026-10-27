using System.Collections.Concurrent;
using ClientFlow.Domain.Entities;
using ClientFlow.Domain.Enums;
using ClientFlow.Domain.Interfaces;

namespace ClientFlow.Infrastructure.Persistence;

public class InMemoryDataStore
{
    public ConcurrentDictionary<Guid, Client> Clients { get; } = new();
    public ConcurrentDictionary<Guid, Project> Projects { get; } = new();
    public ConcurrentDictionary<Guid, ProjectTask> Tasks { get; } = new();
    public ConcurrentDictionary<Guid, TimeEntry> TimeEntries { get; } = new();
    public ConcurrentDictionary<Guid, Invoice> Invoices { get; } = new();
    public ConcurrentBag<AuditLog> AuditLogs { get; } = new();
    private int _invoiceSequence = 1000;

    public int GetNextInvoiceSequence() => Interlocked.Increment(ref _invoiceSequence);
}

public class ClientRepository : IClientRepository
{
    private readonly InMemoryDataStore _store;

    public ClientRepository(InMemoryDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<Client>> GetAllAsync(string? search = null, ClientStatus? status = null, CancellationToken ct = default)
    {
        var query = _store.Clients.Values.AsQueryable();

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(c => c.Name.ToLowerInvariant().Contains(term) ||
                                     c.CompanyName.ToLowerInvariant().Contains(term) ||
                                     c.Email.ToLowerInvariant().Contains(term));
        }

        IReadOnlyList<Client> list = query.OrderByDescending(c => c.CreatedAtUtc).ToList();
        return Task.FromResult(list);
    }

    public Task<Client?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _store.Clients.TryGetValue(id, out var client);
        return Task.FromResult(client);
    }

    public Task AddAsync(Client client, CancellationToken ct = default)
    {
        _store.Clients[client.Id] = client;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Client client, CancellationToken ct = default)
    {
        _store.Clients[client.Id] = client;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _store.Clients.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var exists = _store.Clients.Values.Any(c => c.Email == normalized && (!excludeId.HasValue || c.Id != excludeId.Value));
        return Task.FromResult(exists);
    }
}

public class ProjectRepository : IProjectRepository
{
    private readonly InMemoryDataStore _store;

    public ProjectRepository(InMemoryDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<Project>> GetAllAsync(Guid? clientId = null, ProjectStatus? status = null, CancellationToken ct = default)
    {
        var query = _store.Projects.Values.AsQueryable();

        if (clientId.HasValue)
            query = query.Where(p => p.ClientId == clientId.Value);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        IReadOnlyList<Project> list = query.OrderByDescending(p => p.CreatedAtUtc).ToList();
        return Task.FromResult(list);
    }

    public Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _store.Projects.TryGetValue(id, out var project);
        return Task.FromResult(project);
    }

    public Task AddAsync(Project project, CancellationToken ct = default)
    {
        _store.Projects[project.Id] = project;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Project project, CancellationToken ct = default)
    {
        _store.Projects[project.Id] = project;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _store.Projects.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}

public class TaskRepository : ITaskRepository
{
    private readonly InMemoryDataStore _store;

    public TaskRepository(InMemoryDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<ProjectTask>> GetByProjectIdAsync(Guid projectId, TaskItemStatus? status = null, CancellationToken ct = default)
    {
        var query = _store.Tasks.Values.Where(t => t.ProjectId == projectId);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        IReadOnlyList<ProjectTask> list = query.OrderBy(t => t.DueDateUtc).ThenByDescending(t => t.Priority).ToList();
        return Task.FromResult(list);
    }

    public Task<ProjectTask?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _store.Tasks.TryGetValue(id, out var task);
        return Task.FromResult(task);
    }

    public Task AddAsync(ProjectTask task, CancellationToken ct = default)
    {
        _store.Tasks[task.Id] = task;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ProjectTask task, CancellationToken ct = default)
    {
        _store.Tasks[task.Id] = task;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _store.Tasks.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}

public class TimeEntryRepository : ITimeEntryRepository
{
    private readonly InMemoryDataStore _store;

    public TimeEntryRepository(InMemoryDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<TimeEntry>> GetByProjectIdAsync(Guid projectId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var query = _store.TimeEntries.Values.Where(e => e.ProjectId == projectId);

        if (from.HasValue)
            query = query.Where(e => e.DateUtc >= from.Value.Date);

        if (to.HasValue)
            query = query.Where(e => e.DateUtc <= to.Value.Date);

        IReadOnlyList<TimeEntry> list = query.OrderByDescending(e => e.DateUtc).ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<TimeEntry>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default)
    {
        IReadOnlyList<TimeEntry> list = _store.TimeEntries.Values
            .Where(e => e.ProjectTaskId == taskId)
            .OrderByDescending(e => e.DateUtc)
            .ToList();
        return Task.FromResult(list);
    }

    public Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _store.TimeEntries.TryGetValue(id, out var entry);
        return Task.FromResult(entry);
    }

    public Task AddAsync(TimeEntry entry, CancellationToken ct = default)
    {
        _store.TimeEntries[entry.Id] = entry;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TimeEntry entry, CancellationToken ct = default)
    {
        _store.TimeEntries[entry.Id] = entry;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _store.TimeEntries.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<decimal> GetTotalHoursAsync(Guid projectId, bool? billableOnly = null, CancellationToken ct = default)
    {
        var query = _store.TimeEntries.Values.Where(e => e.ProjectId == projectId);
        if (billableOnly.HasValue)
            query = query.Where(e => e.IsBillable == billableOnly.Value);

        var total = query.Sum(e => e.HoursWorked);
        return Task.FromResult(total);
    }
}

public class InvoiceRepository : IInvoiceRepository
{
    private readonly InMemoryDataStore _store;

    public InvoiceRepository(InMemoryDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<Invoice>> GetAllAsync(Guid? clientId = null, InvoiceStatus? status = null, CancellationToken ct = default)
    {
        var query = _store.Invoices.Values.AsQueryable();

        if (clientId.HasValue)
            query = query.Where(i => i.ClientId == clientId.Value);

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        IReadOnlyList<Invoice> list = query.OrderByDescending(i => i.IssueDateUtc).ToList();
        return Task.FromResult(list);
    }

    public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _store.Invoices.TryGetValue(id, out var inv);
        return Task.FromResult(inv);
    }

    public Task<Invoice?> GetByNumberAsync(string invoiceNumber, CancellationToken ct = default)
    {
        var normalized = invoiceNumber.Trim().ToUpperInvariant();
        var inv = _store.Invoices.Values.FirstOrDefault(i => i.InvoiceNumber == normalized);
        return Task.FromResult(inv);
    }

    public Task AddAsync(Invoice invoice, CancellationToken ct = default)
    {
        _store.Invoices[invoice.Id] = invoice;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Invoice invoice, CancellationToken ct = default)
    {
        _store.Invoices[invoice.Id] = invoice;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _store.Invoices.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<string> GenerateNextInvoiceNumberAsync(CancellationToken ct = default)
    {
        var nextSeq = _store.GetNextInvoiceSequence();
        var year = DateTime.UtcNow.Year;
        return Task.FromResult($"INV-{year}-{nextSeq}");
    }
}

public class AuditLogRepository : IAuditLogRepository
{
    private readonly InMemoryDataStore _store;

    public AuditLogRepository(InMemoryDataStore store)
    {
        _store = store;
    }

    public Task AddAsync(AuditLog log, CancellationToken ct = default)
    {
        _store.AuditLogs.Add(log);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AuditLog>> GetRecentAsync(int count = 50, CancellationToken ct = default)
    {
        IReadOnlyList<AuditLog> list = _store.AuditLogs
            .OrderByDescending(l => l.TimestampUtc)
            .Take(count)
            .ToList();
        return Task.FromResult(list);
    }
}
