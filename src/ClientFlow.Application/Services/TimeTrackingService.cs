using ClientFlow.Application.DTOs;
using ClientFlow.Domain.Entities;
using ClientFlow.Domain.Exceptions;
using ClientFlow.Domain.Interfaces;

namespace ClientFlow.Application.Services;

public class TimeTrackingService : ITimeTrackingService
{
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public TimeTrackingService(
        ITimeEntryRepository timeEntryRepository,
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        IAuditLogRepository auditLogRepository)
    {
        _timeEntryRepository = timeEntryRepository;
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<TimeEntryDto> LogTimeAsync(LogTimeRequest request, string currentUser, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(request.ProjectTaskId, ct)
            ?? throw new EntityNotFoundException(nameof(ProjectTask), request.ProjectTaskId);

        var project = await _projectRepository.GetByIdAsync(request.ProjectId, ct)
            ?? throw new EntityNotFoundException(nameof(Project), request.ProjectId);

        var entry = TimeEntry.Create(
            request.ProjectTaskId,
            request.ProjectId,
            request.UserId,
            request.UserName,
            request.DateUtc,
            request.HoursWorked,
            request.IsBillable,
            request.Notes
        );

        await _timeEntryRepository.AddAsync(entry, ct);
        await _auditLogRepository.AddAsync(AuditLog.Create(nameof(TimeEntry), entry.Id.ToString(), "Create", currentUser, $"Logged {entry.HoursWorked}h on task '{task.Title}' by {entry.UserName}."), ct);

        return new TimeEntryDto(
            entry.Id,
            entry.ProjectTaskId,
            task.Title,
            entry.ProjectId,
            project.Name,
            entry.UserId,
            entry.UserName,
            entry.DateUtc,
            entry.HoursWorked,
            entry.IsBillable,
            entry.Notes,
            entry.IsInvoiced,
            entry.InvoiceId,
            entry.CreatedAtUtc
        );
    }

    public async Task<TimeReportSummaryDto> GetProjectTimeReportAsync(Guid projectId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, ct)
            ?? throw new EntityNotFoundException(nameof(Project), projectId);

        var entries = await _timeEntryRepository.GetByProjectIdAsync(projectId, from, to, ct);
        var dtos = new List<TimeEntryDto>();

        foreach (var e in entries)
        {
            var task = await _taskRepository.GetByIdAsync(e.ProjectTaskId, ct);
            dtos.Add(new TimeEntryDto(
                e.Id,
                e.ProjectTaskId,
                task?.Title ?? "General Task",
                e.ProjectId,
                project.Name,
                e.UserId,
                e.UserName,
                e.DateUtc,
                e.HoursWorked,
                e.IsBillable,
                e.Notes,
                e.IsInvoiced,
                e.InvoiceId,
                e.CreatedAtUtc
            ));
        }

        var totalHours = dtos.Sum(e => e.HoursWorked);
        var billableHours = dtos.Where(e => e.IsBillable).Sum(e => e.HoursWorked);
        var nonBillableHours = totalHours - billableHours;
        var totalBillableAmount = billableHours * project.HourlyRate;

        return new TimeReportSummaryDto(
            totalHours,
            billableHours,
            nonBillableHours,
            totalBillableAmount,
            dtos
        );
    }

    public async Task DeleteTimeEntryAsync(Guid id, string currentUser, CancellationToken ct = default)
    {
        var entry = await _timeEntryRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(TimeEntry), id);

        if (entry.IsInvoiced)
            throw new BusinessRuleValidationException(nameof(TimeEntry), "Cannot delete time entry that has already been billed on an invoice.");

        await _timeEntryRepository.DeleteAsync(id, ct);
        await _auditLogRepository.AddAsync(AuditLog.Create(nameof(TimeEntry), id.ToString(), "Delete", currentUser, $"Removed time entry of {entry.HoursWorked}h."), ct);
    }
}
