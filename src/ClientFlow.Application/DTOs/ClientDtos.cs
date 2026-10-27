using ClientFlow.Domain.Enums;

namespace ClientFlow.Application.DTOs;

public record ClientDto(
    Guid Id,
    string Name,
    string CompanyName,
    string Email,
    string? Phone,
    string? BillingAddress,
    string Currency,
    ClientStatus Status,
    DateTime CreatedAtUtc,
    int ActiveProjectsCount,
    decimal TotalBilled
);

public record CreateClientRequest(
    string Name,
    string CompanyName,
    string Email,
    string? Phone,
    string? BillingAddress,
    string Currency
);

public record UpdateClientRequest(
    string Name,
    string CompanyName,
    string Email,
    string? Phone,
    string? BillingAddress,
    string Currency,
    ClientStatus Status
);
