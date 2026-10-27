using ClientFlow.Domain.Enums;
using ClientFlow.Domain.Exceptions;

namespace ClientFlow.Domain.Entities;

public class Milestone
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime? TargetDateUtc { get; private set; }
    public decimal Amount { get; private set; }
    public MilestoneStatus Status { get; private set; } = MilestoneStatus.Pending;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    private Milestone() { }

    public static Milestone Create(Guid projectId, string title, string? description, DateTime? targetDateUtc, decimal amount)
    {
        if (projectId == Guid.Empty)
            throw new BusinessRuleValidationException(nameof(ProjectId), "Valid project identifier is required.");
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleValidationException(nameof(Title), "Milestone title is required.");
        if (amount < 0)
            throw new BusinessRuleValidationException(nameof(Amount), "Milestone amount cannot be negative.");

        return new Milestone
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = title.Trim(),
            Description = description?.Trim(),
            TargetDateUtc = targetDateUtc,
            Amount = amount,
            Status = MilestoneStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void MarkCompleted()
    {
        Status = MilestoneStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
    }

    public void UpdateStatus(MilestoneStatus status)
    {
        Status = status;
        if (status == MilestoneStatus.Completed && CompletedAtUtc == null)
            CompletedAtUtc = DateTime.UtcNow;
    }
}
