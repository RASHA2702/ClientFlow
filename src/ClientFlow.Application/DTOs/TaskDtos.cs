using ClientFlow.Domain.Enums;

namespace ClientFlow.Application.DTOs;

public record TaskDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string? Description,
    TaskPriority Priority,
    TaskItemStatus Status,
    decimal EstimatedHours,
    decimal LoggedHours,
    string? AssignedTo,
    DateTime? DueDateUtc,
    DateTime CreatedAtUtc
);

public record CreateTaskRequest(
    Guid ProjectId,
    string Title,
    string? Description,
    TaskPriority Priority,
    decimal EstimatedHours,
    string? AssignedTo,
    DateTime? DueDateUtc
);

public record UpdateTaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority,
    decimal EstimatedHours,
    string? AssignedTo,
    DateTime? DueDateUtc
);

public record UpdateTaskStatusRequest(
    TaskItemStatus Status
);
