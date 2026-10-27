using ClientFlow.Application.Common;
using ClientFlow.Application.DTOs;
using ClientFlow.Application.Services;
using ClientFlow.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ClientFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProjectDto>>> GetProjects(
        [FromQuery] Guid? clientId,
        [FromQuery] ProjectStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _projectService.GetProjectsAsync(clientId, status, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> GetProjectById(Guid id, CancellationToken ct = default)
    {
        var project = await _projectService.GetProjectByIdAsync(id, ct);
        return Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectRequest request, CancellationToken ct = default)
    {
        var currentUser = User?.Identity?.Name ?? "AdminUser";
        var project = await _projectService.CreateProjectAsync(request, currentUser, ct);
        return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, project);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken ct = default)
    {
        var currentUser = User?.Identity?.Name ?? "AdminUser";
        var updated = await _projectService.UpdateProjectAsync(id, request, currentUser, ct);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProject(Guid id, CancellationToken ct = default)
    {
        var currentUser = User?.Identity?.Name ?? "AdminUser";
        await _projectService.DeleteProjectAsync(id, currentUser, ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/milestones")]
    public async Task<ActionResult<IReadOnlyList<MilestoneDto>>> GetMilestones(Guid id, CancellationToken ct = default)
    {
        var milestones = await _projectService.GetMilestonesAsync(id, ct);
        return Ok(milestones);
    }

    [HttpPost("{id:guid}/milestones")]
    public async Task<ActionResult<MilestoneDto>> AddMilestone(Guid id, [FromBody] CreateMilestoneRequest request, CancellationToken ct = default)
    {
        var currentUser = User?.Identity?.Name ?? "AdminUser";
        var milestone = await _projectService.AddMilestoneAsync(id, request, currentUser, ct);
        return Ok(milestone);
    }
}
