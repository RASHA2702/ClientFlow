using ClientFlow.Application.DTOs;
using ClientFlow.Application.Services;
using ClientFlow.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ClientFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<IReadOnlyList<TaskDto>>> GetTasksByProject(
        Guid projectId,
        [FromQuery] TaskItemStatus? status,
        CancellationToken ct = default)
    {
        var tasks = await _taskService.GetTasksByProjectAsync(projectId, status, ct);
        return Ok(tasks);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDto>> GetTaskById(Guid id, CancellationToken ct = default)
    {
        var task = await _taskService.GetTaskByIdAsync(id, ct);
        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskRequest request, CancellationToken ct = default)
    {
        var currentUser = User?.Identity?.Name ?? "AdminUser";
        var task = await _taskService.CreateTaskAsync(request, currentUser, ct);
        return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskDto>> UpdateTask(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken ct = default)
    {
        var currentUser = User?.Identity?.Name ?? "AdminUser";
        var updated = await _taskService.UpdateTaskAsync(id, request, currentUser, ct);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<TaskDto>> UpdateTaskStatus(Guid id, [FromBody] UpdateTaskStatusRequest request, CancellationToken ct = default)
    {
        var currentUser = User?.Identity?.Name ?? "AdminUser";
        var updated = await _taskService.UpdateStatusAsync(id, request, currentUser, ct);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTask(Guid id, CancellationToken ct = default)
    {
        var currentUser = User?.Identity?.Name ?? "AdminUser";
        await _taskService.DeleteTaskAsync(id, currentUser, ct);
        return NoContent();
    }
}
