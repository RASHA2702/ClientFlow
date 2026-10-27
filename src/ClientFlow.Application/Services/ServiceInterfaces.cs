using ClientFlow.Application.Common;
using ClientFlow.Application.DTOs;
using ClientFlow.Domain.Enums;

namespace ClientFlow.Application.Services;

public interface IClientService
{
    Task<PagedResult<ClientDto>> GetClientsAsync(string? search = null, ClientStatus? status = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ClientDto> GetClientByIdAsync(Guid id, CancellationToken ct = default);
    Task<ClientDto> CreateClientAsync(CreateClientRequest request, string currentUser, CancellationToken ct = default);
    Task<ClientDto> UpdateClientAsync(Guid id, UpdateClientRequest request, string currentUser, CancellationToken ct = default);
    Task DeleteClientAsync(Guid id, string currentUser, CancellationToken ct = default);
}

public interface IProjectService
{
    Task<PagedResult<ProjectDto>> GetProjectsAsync(Guid? clientId = null, ProjectStatus? status = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ProjectDto> GetProjectByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, string currentUser, CancellationToken ct = default);
    Task<ProjectDto> UpdateProjectAsync(Guid id, UpdateProjectRequest request, string currentUser, CancellationToken ct = default);
    Task DeleteProjectAsync(Guid id, string currentUser, CancellationToken ct = default);
    Task<MilestoneDto> AddMilestoneAsync(Guid projectId, CreateMilestoneRequest request, string currentUser, CancellationToken ct = default);
    Task<IReadOnlyList<MilestoneDto>> GetMilestonesAsync(Guid projectId, CancellationToken ct = default);
}

public interface ITaskService
{
    Task<IReadOnlyList<TaskDto>> GetTasksByProjectAsync(Guid projectId, TaskItemStatus? status = null, CancellationToken ct = default);
    Task<TaskDto> GetTaskByIdAsync(Guid id, CancellationToken ct = default);
    Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, string currentUser, CancellationToken ct = default);
    Task<TaskDto> UpdateTaskAsync(Guid id, UpdateTaskRequest request, string currentUser, CancellationToken ct = default);
    Task<TaskDto> UpdateStatusAsync(Guid id, UpdateTaskStatusRequest request, string currentUser, CancellationToken ct = default);
    Task DeleteTaskAsync(Guid id, string currentUser, CancellationToken ct = default);
}

public interface ITimeTrackingService
{
    Task<TimeEntryDto> LogTimeAsync(LogTimeRequest request, string currentUser, CancellationToken ct = default);
    Task<TimeReportSummaryDto> GetProjectTimeReportAsync(Guid projectId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    Task DeleteTimeEntryAsync(Guid id, string currentUser, CancellationToken ct = default);
}

public interface IInvoiceService
{
    Task<PagedResult<InvoiceDto>> GetInvoicesAsync(Guid? clientId = null, InvoiceStatus? status = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<InvoiceDto> GetInvoiceByIdAsync(Guid id, CancellationToken ct = default);
    Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, string currentUser, CancellationToken ct = default);
    Task<InvoiceDto> MarkSentAsync(Guid id, string currentUser, CancellationToken ct = default);
    Task<InvoiceDto> MarkPaidAsync(Guid id, string currentUser, CancellationToken ct = default);
    Task<InvoiceDto> CancelInvoiceAsync(Guid id, string currentUser, CancellationToken ct = default);
}

public interface IDashboardService
{
    Task<DashboardKpiDto> GetDashboardKpisAsync(CancellationToken ct = default);
}
