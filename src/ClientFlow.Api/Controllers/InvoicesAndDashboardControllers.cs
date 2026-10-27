using ClientFlow.Application.Common;
using ClientFlow.Application.DTOs;
using ClientFlow.Application.Services;
using ClientFlow.Domain.Enums;
using ClientFlow.Infrastructure.Export;
using Microsoft.AspNetCore.Mvc;

namespace ClientFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly ICsvExporter _csvExporter;

    public InvoicesController(IInvoiceService invoiceService, ICsvExporter csvExporter)
    {
        _invoiceService = invoiceService;
        _csvExporter = csvExporter;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<InvoiceDto>>> GetInvoices(
        [FromQuery] Guid? clientId,
        [FromQuery] InvoiceStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _invoiceService.GetInvoicesAsync(clientId, status, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvoiceDto>> GetInvoiceById(Guid id, CancellationToken ct = default)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(id, ct);
        return Ok(invoice);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> CreateInvoice([FromBody] CreateInvoiceRequest request, CancellationToken ct = default)
    {
        var currentUser = User?.Identity?.Name ?? "AdminUser";
        var invoice = await _invoiceService.CreateInvoiceAsync(request, currentUser, ct);
        return CreatedAtAction(nameof(GetInvoiceById), new { id = invoice.Id }, invoice);
    }

    [HttpPost("{id:guid}/send")]
    public async Task<ActionResult<InvoiceDto>> MarkSent(Guid id, CancellationToken ct = default)
    {
        var currentUser = User?.Identity?.Name ?? "AdminUser";
        var invoice = await _invoiceService.MarkSentAsync(id, currentUser, ct);
        return Ok(invoice);
    }

    [HttpPost("{id:guid}/pay")]
    public async Task<ActionResult<InvoiceDto>> MarkPaid(Guid id, CancellationToken ct = default)
    {
        var currentUser = User?.Identity?.Name ?? "AdminUser";
        var invoice = await _invoiceService.MarkPaidAsync(id, currentUser, ct);
        return Ok(invoice);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<InvoiceDto>> CancelInvoice(Guid id, CancellationToken ct = default)
    {
        var currentUser = User?.Identity?.Name ?? "AdminUser";
        var invoice = await _invoiceService.CancelInvoiceAsync(id, currentUser, ct);
        return Ok(invoice);
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportInvoicesCsv(
        [FromQuery] Guid? clientId,
        [FromQuery] InvoiceStatus? status,
        CancellationToken ct = default)
    {
        var result = await _invoiceService.GetInvoicesAsync(clientId, status, 1, 1000, ct);
        var bytes = _csvExporter.ExportInvoices(result.Items);
        return File(bytes, "text/csv", $"invoices-export-{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("kpis")]
    public async Task<ActionResult<DashboardKpiDto>> GetKpis(CancellationToken ct = default)
    {
        var kpis = await _dashboardService.GetDashboardKpisAsync(ct);
        return Ok(kpis);
    }
}
