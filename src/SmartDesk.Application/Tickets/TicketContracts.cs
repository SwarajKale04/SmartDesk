using SmartDesk.Domain.Enums;

namespace SmartDesk.Application.Tickets;

public sealed record CurrentUser(Guid Id, UserRole Role);
public sealed record CreateTicketRequest(string Title, string Description, TicketPriority Priority = TicketPriority.Medium);
public sealed record UpdateTicketRequest(string Title, string Description, TicketPriority Priority);
public sealed record UpdateTicketStatusRequest(TicketStatus Status);
public sealed record AssignTicketRequest(Guid AgentId);
public sealed record AddTicketCommentRequest(string Content, bool IsInternal = false);
public sealed record TicketQuery(int Page = 1, int PageSize = 20, string? Search = null, TicketStatus? Status = null, TicketPriority? Priority = null, Guid? AssignedAgentId = null, string? SortBy = null, bool Descending = true);
public sealed record TicketCommentDto(Guid Id, Guid UserId, string Content, bool IsInternal, DateTimeOffset CreatedAt);
public sealed record TicketHistoryDto(Guid Id, Guid? UserId, string Action, string? OldValue, string? NewValue, DateTimeOffset Timestamp);
public sealed record TicketDto(Guid Id, string TicketNumber, string Title, string Description, TicketPriority Priority, TicketStatus Status, Guid CustomerId, Guid? AssignedAgentId, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, IReadOnlyList<TicketCommentDto>? Comments = null, IReadOnlyList<TicketHistoryDto>? History = null, string? AiPredictedCategory = null, TicketPriority? AiPredictedPriority = null, decimal? AiConfidence = null, bool AiReviewRequired = false, string? AiClassificationStatus = null);
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

public interface ITicketService
{
    Task<TicketDto> CreateAsync(CreateTicketRequest request, CurrentUser currentUser, CancellationToken cancellationToken = default);
    Task<TicketDto> GetByIdAsync(Guid ticketId, CurrentUser currentUser, CancellationToken cancellationToken = default);
    Task<PagedResult<TicketDto>> GetAsync(TicketQuery query, CurrentUser currentUser, CancellationToken cancellationToken = default);
    Task<TicketDto> UpdateAsync(Guid ticketId, UpdateTicketRequest request, CurrentUser currentUser, CancellationToken cancellationToken = default);
    Task<TicketDto> ChangeStatusAsync(Guid ticketId, UpdateTicketStatusRequest request, CurrentUser currentUser, CancellationToken cancellationToken = default);
    Task<TicketDto> AssignAsync(Guid ticketId, AssignTicketRequest request, CurrentUser currentUser, CancellationToken cancellationToken = default);
    Task<TicketCommentDto> AddCommentAsync(Guid ticketId, AddTicketCommentRequest request, CurrentUser currentUser, CancellationToken cancellationToken = default);
}
