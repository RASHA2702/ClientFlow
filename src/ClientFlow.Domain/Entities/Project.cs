using ClientFlow.Domain.Enums;
using ClientFlow.Domain.Exceptions;

namespace ClientFlow.Domain.Entities;

public class Project
{
    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Budget { get; private set; }
    public decimal HourlyRate { get; private set; }
    public ProjectStatus Status { get; private set; } = ProjectStatus.Planning;
    public DateTime? StartDateUtc { get; private set; }
    public DateTime? TargetCompletionDateUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private readonly List<Milestone> _milestones = new();
    public IReadOnlyCollection<Milestone> Milestones => _milestones.AsReadOnly();

    private readonly List<ProjectTask> _tasks = new();
    public IReadOnlyCollection<ProjectTask> Tasks => _tasks.AsReadOnly();

    private Project() { }

    public static Project Create(Guid clientId, string name, string? description, decimal budget, decimal hourlyRate, DateTime? startDateUtc = null, DateTime? targetCompletionDateUtc = null)
    {
        if (clientId == Guid.Empty)
            throw new BusinessRuleValidationException(nameof(ClientId), "Valid client identifier is required.");
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleValidationException(nameof(Name), "Project name is required.");
        if (budget < 0)
            throw new BusinessRuleValidationException(nameof(Budget), "Project budget cannot be negative.");
        if (hourlyRate < 0)
            throw new BusinessRuleValidationException(nameof(HourlyRate), "Hourly rate cannot be negative.");

        return new Project
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            Name = name.Trim(),
            Description = description?.Trim(),
            Budget = budget,
            HourlyRate = hourlyRate,
            Status = ProjectStatus.Planning,
            StartDateUtc = startDateUtc,
            TargetCompletionDateUtc = targetCompletionDateUtc,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Update(string name, string? description, decimal budget, decimal hourlyRate, ProjectStatus status, DateTime? startDateUtc, DateTime? targetCompletionDateUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleValidationException(nameof(Name), "Project name is required.");
        if (budget < 0)
            throw new BusinessRuleValidationException(nameof(Budget), "Project budget cannot be negative.");
        if (hourlyRate < 0)
            throw new BusinessRuleValidationException(nameof(HourlyRate), "Hourly rate cannot be negative.");

        Name = name.Trim();
        Description = description?.Trim();
        Budget = budget;
        HourlyRate = hourlyRate;
        Status = status;
        StartDateUtc = startDateUtc;
        TargetCompletionDateUtc = targetCompletionDateUtc;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void AddMilestone(Milestone milestone)
    {
        if (milestone == null) throw new ArgumentNullException(nameof(milestone));
        _milestones.Add(milestone);
    }

    public void AddTask(ProjectTask task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        _tasks.Add(task);
    }
}
