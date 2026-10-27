using ClientFlow.Application.Common;
using ClientFlow.Application.DTOs;
using ClientFlow.Domain.Entities;
using ClientFlow.Domain.Enums;
using ClientFlow.Domain.Exceptions;
using ClientFlow.Domain.Interfaces;

namespace ClientFlow.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IClientRepository _clientRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public ProjectService(
        IProjectRepository projectRepository,
        IClientRepository clientRepository,
        ITaskRepository taskRepository,
        ITimeEntryRepository timeEntryRepository,
        IAuditLogRepository auditLogRepository)
    {
        _projectRepository = projectRepository;
        _clientRepository = clientRepository;
        _taskRepository = taskRepository;
        _timeEntryRepository = timeEntryRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<PagedResult<ProjectDto>> GetProjectsAsync(Guid? clientId = null, ProjectStatus? status = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var all = await _projectRepository.GetAllAsync(clientId, status, ct);
        var totalCount = all.Count;

        var paged = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var dtos = new List<ProjectDto>();

        foreach (var p in paged)
        {
            var client = await _clientRepository.GetByIdAsync(p.ClientId, ct);
            var tasks = await _taskRepository.GetByProjectIdAsync(p.Id, null, ct);
            var hoursLogged = await _timeEntryRepository.GetTotalHoursAsync(p.Id, null, ct);

            dtos.Add(new ProjectDto(
                p.Id,
                p.ClientId,
                client?.Name ?? "Unknown Client",
                p.Name,
                p.Description,
                p.Budget,
                p.HourlyRate,
                p.Status,
                p.StartDateUtc,
                p.TargetCompletionDateUtc,
                p.CreatedAtUtc,
                hoursLogged,
                hoursLogged * p.HourlyRate,
                tasks.Count,
                tasks.Count(t => t.Status == TaskItemStatus.Done)
            ));
        }

        return new PagedResult<ProjectDto>(dtos, totalCount, page, pageSize);
    }

    public async Task<ProjectDto> GetProjectByIdAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _projectRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Project), id);

        var client = await _clientRepository.GetByIdAsync(p.ClientId, ct);
        var tasks = await _taskRepository.GetByProjectIdAsync(p.Id, null, ct);
        var hoursLogged = await _timeEntryRepository.GetTotalHoursAsync(p.Id, null, ct);

        return new ProjectDto(
            p.Id,
            p.ClientId,
            client?.Name ?? "Unknown Client",
            p.Name,
            p.Description,
            p.Budget,
            p.HourlyRate,
            p.Status,
            p.StartDateUtc,
            p.TargetCompletionDateUtc,
            p.CreatedAtUtc,
            hoursLogged,
            hoursLogged * p.HourlyRate,
            tasks.Count,
            tasks.Count(t => t.Status == TaskItemStatus.Done)
        );
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, string currentUser, CancellationToken ct = default)
    {
        var client = await _clientRepository.GetByIdAsync(request.ClientId, ct)
            ?? throw new EntityNotFoundException(nameof(Client), request.ClientId);

        var project = Project.Create(
            request.ClientId,
            request.Name,
            request.Description,
            request.Budget,
            request.HourlyRate,
            request.StartDateUtc,
            request.TargetCompletionDateUtc
        );

        await _projectRepository.AddAsync(project, ct);
        await _auditLogRepository.AddAsync(AuditLog.Create(nameof(Project), project.Id.ToString(), "Create", currentUser, $"Project '{project.Name}' initialized for client '{client.Name}'."), ct);

        return await GetProjectByIdAsync(project.Id, ct);
    }

    public async Task<ProjectDto> UpdateProjectAsync(Guid id, UpdateProjectRequest request, string currentUser, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Project), id);

        project.Update(
            request.Name,
            request.Description,
            request.Budget,
            request.HourlyRate,
            request.Status,
            request.StartDateUtc,
            request.TargetCompletionDateUtc
        );

        await _projectRepository.UpdateAsync(project, ct);
        await _auditLogRepository.AddAsync(AuditLog.Create(nameof(Project), id.ToString(), "Update", currentUser, $"Project '{project.Name}' updated to status {request.Status}."), ct);

        return await GetProjectByIdAsync(id, ct);
    }

    public async Task DeleteProjectAsync(Guid id, string currentUser, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Project), id);

        await _projectRepository.DeleteAsync(id, ct);
        await _auditLogRepository.AddAsync(AuditLog.Create(nameof(Project), id.ToString(), "Delete", currentUser, $"Project '{project.Name}' deleted."), ct);
    }

    public async Task<MilestoneDto> AddMilestoneAsync(Guid projectId, CreateMilestoneRequest request, string currentUser, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, ct)
            ?? throw new EntityNotFoundException(nameof(Project), projectId);

        var milestone = Milestone.Create(projectId, request.Title, request.Description, request.TargetDateUtc, request.Amount);
        project.AddMilestone(milestone);
        await _projectRepository.UpdateAsync(project, ct);

        await _auditLogRepository.AddAsync(AuditLog.Create(nameof(Milestone), milestone.Id.ToString(), "Create", currentUser, $"Milestone '{milestone.Title}' added to project '{project.Name}'."), ct);

        return new MilestoneDto(
            milestone.Id,
            milestone.ProjectId,
            milestone.Title,
            milestone.Description,
            milestone.TargetDateUtc,
            milestone.Amount,
            milestone.Status,
            milestone.CompletedAtUtc
        );
    }

    public async Task<IReadOnlyList<MilestoneDto>> GetMilestonesAsync(Guid projectId, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, ct)
            ?? throw new EntityNotFoundException(nameof(Project), projectId);

        return project.Milestones.Select(m => new MilestoneDto(
            m.Id,
            m.ProjectId,
            m.Title,
            m.Description,
            m.TargetDateUtc,
            m.Amount,
            m.Status,
            m.CompletedAtUtc
        )).ToList();
    }
}
