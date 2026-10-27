using ClientFlow.Domain.Enums;

namespace ClientFlow.Application.DTOs;

public record ProjectDto(
    Guid Id,
    Guid ClientId,
    string ClientName,
    string Name,
    string? Description,
    decimal Budget,
    decimal HourlyRate,
    ProjectStatus Status,
    DateTime? StartDateUtc,
    DateTime? TargetCompletionDateUtc,
    DateTime CreatedAtUtc,
    decimal TotalHoursLogged,
    decimal TotalCostEstimated,
    int TaskCount,
    int CompletedTaskCount
);

public record CreateProjectRequest(
    Guid ClientId,
    string Name,
    string? Description,
    decimal Budget,
    decimal HourlyRate,
    DateTime? StartDateUtc,
    DateTime? TargetCompletionDateUtc
);

public record UpdateProjectRequest(
    string Name,
    string? Description,
    decimal Budget,
    decimal HourlyRate,
    ProjectStatus Status,
    DateTime? StartDateUtc,
    DateTime? TargetCompletionDateUtc
);

public record MilestoneDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string? Description,
    DateTime? TargetDateUtc,
    decimal Amount,
    MilestoneStatus Status,
    DateTime? CompletedAtUtc
);

public record CreateMilestoneRequest(
    string Title,
    string? Description,
    DateTime? TargetDateUtc,
    decimal Amount
);
