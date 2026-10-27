using ClientFlow.Domain.Enums;
using ClientFlow.Domain.Exceptions;

namespace ClientFlow.Domain.Entities;

public class ProjectTask
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TaskPriority Priority { get; private set; } = TaskPriority.Medium;
    public TaskItemStatus Status { get; private set; } = TaskItemStatus.Todo;
    public decimal EstimatedHours { get; private set; }
    public string? AssignedTo { get; private set; }
    public DateTime? DueDateUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private readonly List<TimeEntry> _timeEntries = new();
    public IReadOnlyCollection<TimeEntry> TimeEntries => _timeEntries.AsReadOnly();

    private ProjectTask() { }

    public static ProjectTask Create(Guid projectId, string title, string? description, TaskPriority priority, decimal estimatedHours, string? assignedTo = null, DateTime? dueDateUtc = null)
    {
        if (projectId == Guid.Empty)
            throw new BusinessRuleValidationException(nameof(ProjectId), "Valid project identifier is required.");
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleValidationException(nameof(Title), "Task title is required.");
        if (estimatedHours < 0)
            throw new BusinessRuleValidationException(nameof(EstimatedHours), "Estimated hours cannot be negative.");

        return new ProjectTask
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = title.Trim(),
            Description = description?.Trim(),
            Priority = priority,
            Status = TaskItemStatus.Todo,
            EstimatedHours = estimatedHours,
            AssignedTo = assignedTo?.Trim(),
            DueDateUtc = dueDateUtc,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void UpdateStatus(TaskItemStatus newStatus)
    {
        // Business Rule: Validate transitions
        Status = newStatus;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateDetails(string title, string? description, TaskPriority priority, decimal estimatedHours, string? assignedTo, DateTime? dueDateUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleValidationException(nameof(Title), "Task title is required.");
        if (estimatedHours < 0)
            throw new BusinessRuleValidationException(nameof(EstimatedHours), "Estimated hours cannot be negative.");

        Title = title.Trim();
        Description = description?.Trim();
        Priority = priority;
        EstimatedHours = estimatedHours;
        AssignedTo = assignedTo?.Trim();
        DueDateUtc = dueDateUtc;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void AddTimeEntry(TimeEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));
        _timeEntries.Add(entry);
    }
}
