using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartDesk.Application.Tickets;
using SmartDesk.Domain.Enums;

namespace SmartDesk.API.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets")]
public sealed class TicketsController(ITicketService ticketService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TicketDto>), StatusCodes.Status200OK)]
    public Task<PagedResult<TicketDto>> Get([FromQuery] TicketQuery query, CancellationToken cancellationToken) => ticketService.GetAsync(query, GetCurrentUser(), cancellationToken);

    [HttpGet("{ticketId:guid}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    public Task<TicketDto> GetById(Guid ticketId, CancellationToken cancellationToken) => ticketService.GetByIdAsync(ticketId, GetCurrentUser(), cancellationToken);

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Customer))]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateTicketRequest request, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.CreateAsync(request, GetCurrentUser(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { ticketId = ticket.Id }, ticket);
    }

    [HttpPut("{ticketId:guid}")]
    public Task<TicketDto> Update(Guid ticketId, UpdateTicketRequest request, CancellationToken cancellationToken) => ticketService.UpdateAsync(ticketId, request, GetCurrentUser(), cancellationToken);

    [HttpPut("{ticketId:guid}/status")]
    [Authorize(Roles = $"{nameof(UserRole.Agent)},{nameof(UserRole.Admin)}")]
    public Task<TicketDto> ChangeStatus(Guid ticketId, UpdateTicketStatusRequest request, CancellationToken cancellationToken) => ticketService.ChangeStatusAsync(ticketId, request, GetCurrentUser(), cancellationToken);

    [HttpPut("{ticketId:guid}/assign")]
    [Authorize(Roles = $"{nameof(UserRole.Agent)},{nameof(UserRole.Admin)}")]
    public Task<TicketDto> Assign(Guid ticketId, AssignTicketRequest request, CancellationToken cancellationToken) => ticketService.AssignAsync(ticketId, request, GetCurrentUser(), cancellationToken);

    [HttpPost("{ticketId:guid}/comments")]
    public Task<TicketCommentDto> AddComment(Guid ticketId, AddTicketCommentRequest request, CancellationToken cancellationToken) => ticketService.AddCommentAsync(ticketId, request, GetCurrentUser(), cancellationToken);

    private CurrentUser GetCurrentUser()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (!Guid.TryParse(id, out var userId) || !Enum.TryParse<UserRole>(role, out var userRole)) throw new UnauthorizedAccessException("Invalid authentication claims.");
        return new CurrentUser(userId, userRole);
    }
}
