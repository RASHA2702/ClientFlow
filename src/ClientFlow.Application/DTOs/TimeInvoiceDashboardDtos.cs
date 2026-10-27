using ClientFlow.Domain.Enums;

namespace ClientFlow.Application.DTOs;

public record TimeEntryDto(
    Guid Id,
    Guid ProjectTaskId,
    string TaskTitle,
    Guid ProjectId,
    string ProjectName,
    string UserId,
    string UserName,
    DateTime DateUtc,
    decimal HoursWorked,
    bool IsBillable,
    string? Notes,
    bool IsInvoiced,
    Guid? InvoiceId,
    DateTime CreatedAtUtc
);

public record LogTimeRequest(
    Guid ProjectTaskId,
    Guid ProjectId,
    string UserId,
    string UserName,
    DateTime DateUtc,
    decimal HoursWorked,
    bool IsBillable,
    string? Notes
);

public record TimeReportSummaryDto(
    decimal TotalHours,
    decimal BillableHours,
    decimal NonBillableHours,
    decimal TotalBillableAmount,
    IReadOnlyList<TimeEntryDto> Entries
);

public record InvoiceItemDto(
    Guid Id,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal
);

public record InvoiceDto(
    Guid Id,
    Guid ClientId,
    string ClientName,
    Guid? ProjectId,
    string? ProjectName,
    string InvoiceNumber,
    DateTime IssueDateUtc,
    DateTime DueDateUtc,
    decimal SubTotal,
    decimal TaxRatePercentage,
    decimal TaxAmount,
    decimal TotalAmount,
    InvoiceStatus Status,
    string Currency,
    string? Notes,
    DateTime CreatedAtUtc,
    DateTime? PaidAtUtc,
    IReadOnlyList<InvoiceItemDto> Items
);

public record CreateInvoiceRequest(
    Guid ClientId,
    Guid? ProjectId,
    DateTime IssueDateUtc,
    DateTime DueDateUtc,
    decimal TaxRatePercentage,
    string Currency,
    string? Notes,
    List<CreateInvoiceItemRequest> Items
);

public record CreateInvoiceItemRequest(
    string Description,
    decimal Quantity,
    decimal UnitPrice
);

public record DashboardKpiDto(
    int TotalActiveClients,
    int TotalActiveProjects,
    int OpenTasksCount,
    decimal TotalHoursLoggedThisMonth,
    decimal TotalRevenueCollected,
    decimal TotalOutstandingInvoicesAmount,
    IReadOnlyList<ProjectDto> RecentProjects,
    IReadOnlyList<TaskDto> UrgentTasks,
    IReadOnlyList<InvoiceDto> RecentInvoices
);
