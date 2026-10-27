using ClientFlow.Domain.Exceptions;

namespace ClientFlow.Domain.Entities;

public class TimeEntry
{
    public Guid Id { get; private set; }
    public Guid ProjectTaskId { get; private set; }
    public Guid ProjectId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public DateTime DateUtc { get; private set; }
    public decimal HoursWorked { get; private set; }
    public bool IsBillable { get; private set; } = true;
    public string? Notes { get; private set; }
    public bool IsInvoiced { get; private set; }
    public Guid? InvoiceId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private TimeEntry() { }

    public static TimeEntry Create(Guid projectTaskId, Guid projectId, string userId, string userName, DateTime dateUtc, decimal hoursWorked, bool isBillable, string? notes)
    {
        if (projectTaskId == Guid.Empty)
            throw new BusinessRuleValidationException(nameof(ProjectTaskId), "Task identifier is required.");
        if (string.IsNullOrWhiteSpace(userId))
            throw new BusinessRuleValidationException(nameof(UserId), "User identifier is required.");
        if (hoursWorked <= 0 || hoursWorked > 24)
            throw new BusinessRuleValidationException(nameof(HoursWorked), "Hours worked must be greater than 0 and at most 24 hours per entry.");

        return new TimeEntry
        {
            Id = Guid.NewGuid(),
            ProjectTaskId = projectTaskId,
            ProjectId = projectId,
            UserId = userId.Trim(),
            UserName = userName?.Trim() ?? "User",
            DateUtc = dateUtc.Date,
            HoursWorked = hoursWorked,
            IsBillable = isBillable,
            Notes = notes?.Trim(),
            IsInvoiced = false,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void MarkInvoiced(Guid invoiceId)
    {
        if (invoiceId == Guid.Empty)
            throw new BusinessRuleValidationException(nameof(invoiceId), "Invoice identifier is required.");
        IsInvoiced = true;
        InvoiceId = invoiceId;
    }

    public void UnmarkInvoiced()
    {
        IsInvoiced = false;
        InvoiceId = null;
    }
}
