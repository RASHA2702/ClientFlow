using ClientFlow.Application.Common;
using ClientFlow.Application.DTOs;
using ClientFlow.Domain.Entities;
using ClientFlow.Domain.Enums;
using ClientFlow.Domain.Exceptions;
using ClientFlow.Domain.Interfaces;

namespace ClientFlow.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public InvoiceService(
        IInvoiceRepository invoiceRepository,
        IClientRepository clientRepository,
        IProjectRepository projectRepository,
        IAuditLogRepository auditLogRepository)
    {
        _invoiceRepository = invoiceRepository;
        _clientRepository = clientRepository;
        _projectRepository = projectRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<PagedResult<InvoiceDto>> GetInvoicesAsync(Guid? clientId = null, InvoiceStatus? status = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var all = await _invoiceRepository.GetAllAsync(clientId, status, ct);
        var totalCount = all.Count;

        var paged = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var dtos = new List<InvoiceDto>();

        foreach (var inv in paged)
        {
            var client = await _clientRepository.GetByIdAsync(inv.ClientId, ct);
            Project? project = inv.ProjectId.HasValue ? await _projectRepository.GetByIdAsync(inv.ProjectId.Value, ct) : null;

            dtos.Add(MapToDto(inv, client?.Name ?? "Client", project?.Name));
        }

        return new PagedResult<InvoiceDto>(dtos, totalCount, page, pageSize);
    }

    public async Task<InvoiceDto> GetInvoiceByIdAsync(Guid id, CancellationToken ct = default)
    {
        var inv = await _invoiceRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Invoice), id);

        var client = await _clientRepository.GetByIdAsync(inv.ClientId, ct);
        Project? project = inv.ProjectId.HasValue ? await _projectRepository.GetByIdAsync(inv.ProjectId.Value, ct) : null;

        return MapToDto(inv, client?.Name ?? "Client", project?.Name);
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, string currentUser, CancellationToken ct = default)
    {
        var client = await _clientRepository.GetByIdAsync(request.ClientId, ct)
            ?? throw new EntityNotFoundException(nameof(Client), request.ClientId);

        var nextNumber = await _invoiceRepository.GenerateNextInvoiceNumberAsync(ct);
        var invoice = Invoice.Create(
            request.ClientId,
            request.ProjectId,
            nextNumber,
            request.IssueDateUtc,
            request.DueDateUtc,
            request.TaxRatePercentage,
            request.Currency,
            request.Notes
        );

        if (request.Items != null)
        {
            foreach (var item in request.Items)
            {
                invoice.AddItem(item.Description, item.Quantity, item.UnitPrice);
            }
        }

        await _invoiceRepository.AddAsync(invoice, ct);
        await _auditLogRepository.AddAsync(AuditLog.Create(nameof(Invoice), invoice.Id.ToString(), "Create", currentUser, $"Invoice '{invoice.InvoiceNumber}' issued for {invoice.TotalAmount:C}."), ct);

        return await GetInvoiceByIdAsync(invoice.Id, ct);
    }

    public async Task<InvoiceDto> MarkSentAsync(Guid id, string currentUser, CancellationToken ct = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Invoice), id);

        invoice.MarkSent();
        await _invoiceRepository.UpdateAsync(invoice, ct);
        await _auditLogRepository.AddAsync(AuditLog.Create(nameof(Invoice), id.ToString(), "StatusChange", currentUser, $"Invoice '{invoice.InvoiceNumber}' marked as Sent."), ct);

        return await GetInvoiceByIdAsync(id, ct);
    }

    public async Task<InvoiceDto> MarkPaidAsync(Guid id, string currentUser, CancellationToken ct = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Invoice), id);

        invoice.MarkPaid();
        await _invoiceRepository.UpdateAsync(invoice, ct);
        await _auditLogRepository.AddAsync(AuditLog.Create(nameof(Invoice), id.ToString(), "PaymentReceived", currentUser, $"Invoice '{invoice.InvoiceNumber}' marked as Paid ({invoice.TotalAmount:C})."), ct);

        return await GetInvoiceByIdAsync(id, ct);
    }

    public async Task<InvoiceDto> CancelInvoiceAsync(Guid id, string currentUser, CancellationToken ct = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Invoice), id);

        invoice.MarkCancelled();
        await _invoiceRepository.UpdateAsync(invoice, ct);
        await _auditLogRepository.AddAsync(AuditLog.Create(nameof(Invoice), id.ToString(), "Cancelled", currentUser, $"Invoice '{invoice.InvoiceNumber}' cancelled."), ct);

        return await GetInvoiceByIdAsync(id, ct);
    }

    private static InvoiceDto MapToDto(Invoice inv, string clientName, string? projectName)
    {
        var items = inv.Items.Select(i => new InvoiceItemDto(
            i.Id,
            i.Description,
            i.Quantity,
            i.UnitPrice,
            i.LineTotal
        )).ToList();

        return new InvoiceDto(
            inv.Id,
            inv.ClientId,
            clientName,
            inv.ProjectId,
            projectName,
            inv.InvoiceNumber,
            inv.IssueDateUtc,
            inv.DueDateUtc,
            inv.SubTotal,
            inv.TaxRatePercentage,
            inv.TaxAmount,
            inv.TotalAmount,
            inv.Status,
            inv.Currency,
            inv.Notes,
            inv.CreatedAtUtc,
            inv.PaidAtUtc,
            items
        );
    }
}
