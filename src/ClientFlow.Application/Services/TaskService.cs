using ClientFlow.Application.DTOs;
using ClientFlow.Domain.Entities;
using ClientFlow.Domain.Enums;
using ClientFlow.Domain.Exceptions;
using ClientFlow.Domain.Interfaces;

namespace ClientFlow.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public TaskService(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        ITimeEntryRepository timeEntryRepository,
        IAuditLogRepository auditLogRepository)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _timeEntryRepository = timeEntryRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<IReadOnlyList<TaskDto>> GetTasksByProjectAsync(Guid projectId, TaskItemStatus? status = null, CancellationToken ct = default)
    {
        var tasks = await _taskRepository.GetByProjectIdAsync(projectId, status, ct);
        var dtos = new List<TaskDto>();

        foreach (var t in tasks)
        {
            var entries = await _timeEntryRepository.GetByTaskIdAsync(t.Id, ct);
            var loggedHours = entries.Sum(e => e.HoursWorked);

            dtos.Add(new TaskDto(
                t.Id,
                t.ProjectId,
                t.Title,
                t.Description,
                t.Priority,
                t.Status,
                t.EstimatedHours,
                loggedHours,
                t.AssignedTo,
                t.DueDateUtc,
                t.CreatedAtUtc
            ));
        }

        return dtos;
    }

    public async Task<TaskDto> GetTaskByIdAsync(Guid id, CancellationToken ct = default)
    {
        var t = await _taskRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(ProjectTask), id);

        var entries = await _timeEntryRepository.GetByTaskIdAsync(t.Id, ct);
        var loggedHours = entries.Sum(e => e.HoursWorked);

        return new TaskDto(
            t.Id,
            t.ProjectId,
            t.Title,
            t.Description,
            t.Priority,
            t.Status,
            t.EstimatedHours,
            loggedHours,
            t.AssignedTo,
            t.DueDateUtc,
            t.CreatedAtUtc
        );
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, string currentUser, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, ct)
            ?? throw new EntityNotFoundException(nameof(Project), request.ProjectId);

        var task = ProjectTask.Create(
            request.ProjectId,
            request.Title,
            request.Description,
            request.Priority,
            request.EstimatedHours,
            request.AssignedTo,
            request.DueDateUtc
        );

        await _taskRepository.AddAsync(task, ct);
        await _auditLogRepository.AddAsync(AuditLog.Create(nameof(ProjectTask), task.Id.ToString(), "Create", currentUser, $"Task '{task.Title}' added to project '{project.Name}'."), ct);

        return await GetTaskByIdAsync(task.Id, ct);
    }

    public async Task<TaskDto> UpdateTaskAsync(Guid id, UpdateTaskRequest request, string currentUser, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(ProjectTask), id);

        task.UpdateDetails(
            request.Title,
            request.Description,
            request.Priority,
            request.EstimatedHours,
            request.AssignedTo,
            request.DueDateUtc
        );

        await _taskRepository.UpdateAsync(task, ct);
        await _auditLogRepository.AddAsync(AuditLog.Create(nameof(ProjectTask), task.Id.ToString(), "Update", currentUser, $"Task '{task.Title}' details updated."), ct);

        return await GetTaskByIdAsync(id, ct);
    }

    public async Task<TaskDto> UpdateStatusAsync(Guid id, UpdateTaskStatusRequest request, string currentUser, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(ProjectTask), id);

        var previousStatus = task.Status;
        task.UpdateStatus(request.Status);

        await _taskRepository.UpdateAsync(task, ct);
        await _auditLogRepository.AddAsync(AuditLog.Create(nameof(ProjectTask), task.Id.ToString(), "StatusTransition", currentUser, $"Task '{task.Title}' changed from {previousStatus} to {request.Status}."), ct);

        return await GetTaskByIdAsync(id, ct);
    }

    public async Task DeleteTaskAsync(Guid id, string currentUser, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(ProjectTask), id);

        await _taskRepository.DeleteAsync(id, ct);
        await _auditLogRepository.AddAsync(AuditLog.Create(nameof(ProjectTask), id.ToString(), "Delete", currentUser, $"Task '{task.Title}' deleted."), ct);
    }
}
