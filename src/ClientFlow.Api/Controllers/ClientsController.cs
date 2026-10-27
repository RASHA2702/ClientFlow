using ClientFlow.Application.Common;
using ClientFlow.Application.DTOs;
using ClientFlow.Application.Services;
using ClientFlow.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ClientFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ClientDto>>> GetClients(
        [FromQuery] string? search,
        [FromQuery] ClientStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _clientService.GetClientsAsync(search, status, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClientDto>> GetClientById(Guid id, CancellationToken ct = default)
    {
        var client = await _clientService.GetClientByIdAsync(id, ct);
        return Ok(client);
    }

    [HttpPost]
    public async Task<ActionResult<ClientDto>> CreateClient([FromBody] CreateClientRequest request, CancellationToken ct = default)
    {
        var currentUser = User?.Identity?.Name ?? "AdminUser";
        var client = await _clientService.CreateClientAsync(request, currentUser, ct);
        return CreatedAtAction(nameof(GetClientById), new { id = client.Id }, client);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClientDto>> UpdateClient(Guid id, [FromBody] UpdateClientRequest request, CancellationToken ct = default)
    {
        var currentUser = User?.Identity?.Name ?? "AdminUser";
        var updated = await _clientService.UpdateClientAsync(id, request, currentUser, ct);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteClient(Guid id, CancellationToken ct = default)
    {
        var currentUser = User?.Identity?.Name ?? "AdminUser";
        await _clientService.DeleteClientAsync(id, currentUser, ct);
        return NoContent();
    }
}
