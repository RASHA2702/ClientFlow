namespace ClientFlow.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; private set; }
    public string EntityName { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty; // Create, Update, Delete, Transition
    public string PerformedBy { get; private set; } = string.Empty;
    public DateTime TimestampUtc { get; private set; }
    public string? Details { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(string entityName, string entityId, string action, string performedBy, string? details = null)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            PerformedBy = string.IsNullOrWhiteSpace(performedBy) ? "System" : performedBy.Trim(),
            TimestampUtc = DateTime.UtcNow,
            Details = details
        };
    }
}
