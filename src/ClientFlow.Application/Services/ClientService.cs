using ClientFlow.Application.Common;
using ClientFlow.Application.DTOs;
using ClientFlow.Domain.Entities;
using ClientFlow.Domain.Enums;
using ClientFlow.Domain.Exceptions;
using ClientFlow.Domain.Interfaces;

namespace ClientFlow.Application.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public ClientService(
        IClientRepository clientRepository,
        IProjectRepository projectRepository,
        IInvoiceRepository invoiceRepository,
        IAuditLogRepository auditLogRepository)
    {
        _clientRepository = clientRepository;
        _projectRepository = projectRepository;
        _invoiceRepository = invoiceRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<PagedResult<ClientDto>> GetClientsAsync(string? search = null, ClientStatus? status = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var allClients = await _clientRepository.GetAllAsync(search, status, ct);
        var totalCount = allClients.Count;

        var pagedItems = allClients
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = new List<ClientDto>();
        foreach (var c in pagedItems)
        {
            var projects = await _projectRepository.GetAllAsync(c.Id, null, ct);
            var activeProjects = projects.Count(p => p.Status == ProjectStatus.Active || p.Status == ProjectStatus.Planning);
            var invoices = await _invoiceRepository.GetAllAsync(c.Id, InvoiceStatus.Paid, ct);
            var totalBilled = invoices.Sum(i => i.TotalAmount);

            dtos.Add(new ClientDto(
                c.Id,
                c.Name,
                c.CompanyName,
                c.Email,
                c.Phone,
                c.BillingAddress,
                c.Currency,
                c.Status,
                c.CreatedAtUtc,
                activeProjects,
                totalBilled
            ));
        }

        return new PagedResult<ClientDto>(dtos, totalCount, page, pageSize);
    }

    public async Task<ClientDto> GetClientByIdAsync(Guid id, CancellationToken ct = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Client), id);

        var projects = await _projectRepository.GetAllAsync(client.Id, null, ct);
        var activeProjects = projects.Count(p => p.Status == ProjectStatus.Active || p.Status == ProjectStatus.Planning);
        var invoices = await _invoiceRepository.GetAllAsync(client.Id, InvoiceStatus.Paid, ct);
        var totalBilled = invoices.Sum(i => i.TotalAmount);

        return new ClientDto(
            client.Id,
            client.Name,
            client.CompanyName,
            client.Email,
            client.Phone,
            client.BillingAddress,
            client.Currency,
            client.Status,
            client.CreatedAtUtc,
            activeProjects,
            totalBilled
        );
    }

    public async Task<ClientDto> CreateClientAsync(CreateClientRequest request, string currentUser, CancellationToken ct = default)
    {
        var exists = await _clientRepository.ExistsByEmailAsync(request.Email, null, ct);
        if (exists)
            throw new BusinessRuleValidationException(nameof(request.Email), $"A client with email '{request.Email}' already exists.");

        var client = Client.Create(request.Name, request.CompanyName, request.Email, request.Phone, request.BillingAddress, request.Currency);
        await _clientRepository.AddAsync(client, ct);

        await _auditLogRepository.AddAsync(AuditLog.Create(nameof(Client), client.Id.ToString(), "Create", currentUser, $"Client '{client.Name}' created."), ct);

        return new ClientDto(
            client.Id,
            client.Name,
            client.CompanyName,
            client.Email,
            client.Phone,
            client.BillingAddress,
            client.Currency,
            client.Status,
            client.CreatedAtUtc,
            0,
            0
        );
    }

    public async Task<ClientDto> UpdateClientAsync(Guid id, UpdateClientRequest request, string currentUser, CancellationToken ct = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Client), id);

        var exists = await _clientRepository.ExistsByEmailAsync(request.Email, id, ct);
        if (exists)
            throw new BusinessRuleValidationException(nameof(request.Email), $"Another client with email '{request.Email}' already exists.");

        client.UpdateDetails(request.Name, request.CompanyName, request.Email, request.Phone, request.BillingAddress, request.Currency);
        client.SetStatus(request.Status);

        await _clientRepository.UpdateAsync(client, ct);
        await _auditLogRepository.AddAsync(AuditLog.Create(nameof(Client), client.Id.ToString(), "Update", currentUser, $"Client '{client.Name}' updated."), ct);

        return await GetClientByIdAsync(id, ct);
    }

    public async Task DeleteClientAsync(Guid id, string currentUser, CancellationToken ct = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Client), id);

        var projects = await _projectRepository.GetAllAsync(client.Id, null, ct) ?? Array.Empty<Project>();
        if (projects.Any(p => p.Status == ProjectStatus.Active))
            throw new BusinessRuleValidationException(nameof(Client), "Cannot delete client with active ongoing projects.");

        await _clientRepository.DeleteAsync(id, ct);
        await _auditLogRepository.AddAsync(AuditLog.Create(nameof(Client), id.ToString(), "Delete", currentUser, $"Client '{client.Name}' removed."), ct);
    }
}
