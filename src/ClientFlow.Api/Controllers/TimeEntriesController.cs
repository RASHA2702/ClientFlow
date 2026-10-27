using ClientFlow.Application.DTOs;
using ClientFlow.Application.Services;
using ClientFlow.Infrastructure.Export;
using Microsoft.AspNetCore.Mvc;

namespace ClientFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TimeEntriesController : ControllerBase
{
    private readonly ITimeTrackingService _timeTrackingService;
    private readonly ICsvExporter _csvExporter;

    public TimeEntriesController(ITimeTrackingService timeTrackingService, ICsvExporter csvExporter)
    {
        _timeTrackingService = timeTrackingService;
        _csvExporter = csvExporter;
    }

    [HttpPost]
    public async Task<ActionResult<TimeEntryDto>> LogTime([FromBody] LogTimeRequest request, CancellationToken ct = default)
    {
        var currentUser = User?.Identity?.Name ?? "AdminUser";
        var entry = await _timeTrackingService.LogTimeAsync(request, currentUser, ct);
        return Ok(entry);
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<TimeReportSummaryDto>> GetProjectTimeReport(
        Guid projectId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct = default)
    {
        var report = await _timeTrackingService.GetProjectTimeReportAsync(projectId, from, to, ct);
        return Ok(report);
    }

    [HttpGet("project/{projectId:guid}/export")]
    public async Task<IActionResult> ExportTimeCsv(
        Guid projectId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct = default)
    {
        var report = await _timeTrackingService.GetProjectTimeReportAsync(projectId, from, to, ct);
        var bytes = _csvExporter.ExportTimeEntries(report.Entries);
        return File(bytes, "text/csv", $"timesheet-project-{projectId:N}.csv");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTimeEntry(Guid id, CancellationToken ct = default)
    {
        var currentUser = User?.Identity?.Name ?? "AdminUser";
        await _timeTrackingService.DeleteTimeEntryAsync(id, currentUser, ct);
        return NoContent();
    }
}
