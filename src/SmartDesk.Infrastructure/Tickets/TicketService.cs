using Microsoft.EntityFrameworkCore;
using SmartDesk.Application.Common;
using SmartDesk.Application.Tickets;
using SmartDesk.Application.Sla;
using SmartDesk.Application.Ai;
using SmartDesk.Application.Notifications;
using SmartDesk.Domain.Entities;
using SmartDesk.Domain.Enums;
using SmartDesk.Infrastructure.Persistence;
using SmartDesk.Infrastructure.Ai;
using Microsoft.Extensions.Options;

namespace SmartDesk.Infrastructure.Tickets;

public sealed class TicketService(SmartDeskDbContext dbContext, ISlaCalculationService slaCalculationService, ITicketClassificationService classificationService, INotificationService notificationService, IOptions<AiClassificationOptions> aiOptions) : ITicketService
{
    public async Task<TicketDto> CreateAsync(CreateTicketRequest request, CurrentUser currentUser, CancellationToken cancellationToken = default)
    {
        if (currentUser.Role != UserRole.Customer) throw new ForbiddenException("Only customers can create tickets.");
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description)) throw new ValidationException("Title and description are required.");
        if (request.Title.Length > 250 || request.Description.Length > 8000) throw new ValidationException("Ticket content exceeds the allowed length.");
        var ticket = Ticket.Create(await CreateNumberAsync(cancellationToken), request.Title, request.Description, currentUser.Id, request.Priority);
        try
        {
            var classification = await classificationService.ClassifyAsync(request.Title, request.Description, cancellationToken);
            var applyPrediction = classification.Confidence >= aiOptions.Value.ApplyConfidenceThreshold;
            ticket.ApplyAiClassification(classification.Category, classification.Priority, classification.Confidence, applyPrediction, classification.Confidence < aiOptions.Value.ReviewConfidenceThreshold);
            if (applyPrediction)
            {
                var category = await dbContext.Categories.AsNoTracking().SingleOrDefaultAsync(x => x.Name == classification.Category && x.IsActive, cancellationToken);
                if (category is not null) ticket.SetCategory(category.Id);
            }
            dbContext.TicketHistories.Add(TicketHistory.Create(ticket.Id, currentUser.Id, "AiClassified", null, $"{classification.Category}/{classification.Priority}/{classification.Confidence:P0}"));
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            dbContext.TicketHistories.Add(TicketHistory.Create(ticket.Id, currentUser.Id, "AiClassificationUnavailable"));
        }
        var sla = await slaCalculationService.CalculateAsync(ticket.Priority, ticket.CreatedAt, cancellationToken);
        if (sla is not null)
        {
            ticket.ApplySla(sla.PolicyId, sla.FirstResponseDueAt, sla.ResolutionDueAt);
            dbContext.TicketHistories.Add(TicketHistory.Create(ticket.Id, currentUser.Id, "SlaApplied", null, ticket.DueAt?.ToString("O")));
        }
        dbContext.Tickets.Add(ticket);
        dbContext.TicketHistories.Add(TicketHistory.Create(ticket.Id, currentUser.Id, "TicketCreated", null, ticket.Status.ToString()));
        await dbContext.SaveChangesAsync(cancellationToken);
        var admins = await dbContext.Users.AsNoTracking().Where(x => x.Role == UserRole.Admin && x.IsActive).Select(x => x.Id).ToListAsync(cancellationToken);
        foreach (var adminId in admins) await notificationService.NotifyAsync(adminId, "TicketCreated", $"New {ticket.Priority} ticket {ticket.TicketNumber} was created.", ticket.Id, cancellationToken);
        return Map(ticket);
    }

    public async Task<TicketDto> GetByIdAsync(Guid ticketId, CurrentUser currentUser, CancellationToken cancellationToken = default)
    {
        var ticket = await dbContext.Tickets.AsNoTracking().SingleOrDefaultAsync(x => x.Id == ticketId, cancellationToken) ?? throw new NotFoundException("Ticket not found.");
        EnsureCanView(ticket, currentUser);
        var comments = await dbContext.TicketComments.AsNoTracking().Where(x => x.TicketId == ticketId && (currentUser.Role != UserRole.Customer || !x.IsInternal)).OrderBy(x => x.CreatedAt).Select(x => new TicketCommentDto(x.Id, x.UserId, x.Content, x.IsInternal, x.CreatedAt)).ToListAsync(cancellationToken);
        var history = await dbContext.TicketHistories.AsNoTracking().Where(x => x.TicketId == ticketId).OrderBy(x => x.Timestamp).Select(x => new TicketHistoryDto(x.Id, x.UserId, x.Action, x.OldValue, x.NewValue, x.Timestamp)).ToListAsync(cancellationToken);
        return Map(ticket, comments, history);
    }

    public async Task<PagedResult<TicketDto>> GetAsync(TicketQuery query, CurrentUser currentUser, CancellationToken cancellationToken = default)
    {
        if (query.Page < 1 || query.PageSize is < 1 or > 100) throw new ValidationException("Page must be positive and page size must be between 1 and 100.");
        var tickets = dbContext.Tickets.AsNoTracking().AsQueryable();
        tickets = currentUser.Role switch
        {
            UserRole.Customer => tickets.Where(x => x.CustomerId == currentUser.Id),
            UserRole.Agent => tickets.Where(x => x.AssignedAgentId == currentUser.Id || x.AssignedAgentId == null),
            _ => tickets
        };
        if (!string.IsNullOrWhiteSpace(query.Search)) { var search = query.Search.Trim(); tickets = tickets.Where(x => x.TicketNumber.Contains(search) || x.Title.Contains(search)); }
        if (query.Status is not null) tickets = tickets.Where(x => x.Status == query.Status);
        if (query.Priority is not null) tickets = tickets.Where(x => x.Priority == query.Priority);
        if (query.AssignedAgentId is not null && currentUser.Role != UserRole.Customer) tickets = tickets.Where(x => x.AssignedAgentId == query.AssignedAgentId);
        var total = await tickets.CountAsync(cancellationToken);
        tickets = (query.SortBy?.ToLowerInvariant(), query.Descending) switch
        {
            ("priority", false) => tickets.OrderBy(x => x.Priority), ("priority", true) => tickets.OrderByDescending(x => x.Priority),
            ("updatedat", false) => tickets.OrderBy(x => x.UpdatedAt), ("updatedat", true) => tickets.OrderByDescending(x => x.UpdatedAt),
            _ when query.Descending => tickets.OrderByDescending(x => x.CreatedAt), _ => tickets.OrderBy(x => x.CreatedAt)
        };
        var items = await tickets.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(cancellationToken);
        return new PagedResult<TicketDto>(items.Select(x => Map(x)).ToList(), query.Page, query.PageSize, total);
    }

    public async Task<TicketDto> ChangeStatusAsync(Guid ticketId, UpdateTicketStatusRequest request, CurrentUser currentUser, CancellationToken cancellationToken = default)
    {
        var ticket = await GetForWorkAsync(ticketId, currentUser, cancellationToken);
        var previous = ticket.Status;
        try { ticket.ChangeStatus(request.Status, DateTimeOffset.UtcNow); }
        catch (InvalidOperationException exception) { throw new ValidationException(exception.Message); }
        dbContext.TicketHistories.Add(TicketHistory.Create(ticket.Id, currentUser.Id, "StatusChanged", previous.ToString(), request.Status.ToString()));
        if (request.Status == TicketStatus.Reopened)
        {
            var reopenedSla = await slaCalculationService.CalculateAsync(ticket.Priority, DateTimeOffset.UtcNow, cancellationToken);
            if (reopenedSla is not null)
            {
                ticket.ApplySla(reopenedSla.PolicyId, reopenedSla.FirstResponseDueAt, reopenedSla.ResolutionDueAt);
                dbContext.TicketHistories.Add(TicketHistory.Create(ticket.Id, currentUser.Id, "SlaReapplied"));
            }
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyAsync(ticket.CustomerId, "TicketStatusChanged", $"Your ticket {ticket.TicketNumber} is now {ticket.Status}.", ticket.Id, cancellationToken);
        return Map(ticket);
    }

    public async Task<TicketDto> UpdateAsync(Guid ticketId, UpdateTicketRequest request, CurrentUser currentUser, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description) || request.Title.Length > 250 || request.Description.Length > 8000)
            throw new ValidationException("Title and description are required and must be within their allowed lengths.");
        var ticket = await dbContext.Tickets.SingleOrDefaultAsync(x => x.Id == ticketId, cancellationToken) ?? throw new NotFoundException("Ticket not found.");
        EnsureCanView(ticket, currentUser);
        if (currentUser.Role == UserRole.Agent && ticket.AssignedAgentId != currentUser.Id) throw new ForbiddenException("Agents can update only assigned tickets.");
        if (currentUser.Role == UserRole.Customer && ticket.Status != TicketStatus.New) throw new ForbiddenException("Customers can update tickets only while they are new.");
        var previousPriority = ticket.Priority;
        try { ticket.UpdateDetails(request.Title, request.Description, request.Priority, DateTimeOffset.UtcNow); } catch (Exception exception) when (exception is ArgumentException or InvalidOperationException) { throw new ValidationException(exception.Message); }
        if (previousPriority != request.Priority) dbContext.TicketHistories.Add(TicketHistory.Create(ticket.Id, currentUser.Id, "PriorityChanged", previousPriority.ToString(), request.Priority.ToString()));
        dbContext.TicketHistories.Add(TicketHistory.Create(ticket.Id, currentUser.Id, "TicketUpdated"));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(ticket);
    }

    public async Task<TicketDto> AssignAsync(Guid ticketId, AssignTicketRequest request, CurrentUser currentUser, CancellationToken cancellationToken = default)
    {
        var ticket = await dbContext.Tickets.SingleOrDefaultAsync(x => x.Id == ticketId, cancellationToken) ?? throw new NotFoundException("Ticket not found.");
        if (currentUser.Role == UserRole.Agent && (request.AgentId != currentUser.Id || ticket.AssignedAgentId is not null)) throw new ForbiddenException("Agents can only claim an unassigned ticket for themselves.");
        if (currentUser.Role == UserRole.Customer) throw new ForbiddenException("Customers cannot assign tickets.");
        var agentExists = await dbContext.Users.AnyAsync(x => x.Id == request.AgentId && x.Role == UserRole.Agent && x.IsActive, cancellationToken);
        if (!agentExists) throw new ValidationException("Assigned user must be an active agent.");
        var previous = ticket.AssignedAgentId?.ToString();
        try { ticket.AssignTo(request.AgentId, DateTimeOffset.UtcNow); } catch (InvalidOperationException exception) { throw new ValidationException(exception.Message); }
        dbContext.TicketHistories.Add(TicketHistory.Create(ticket.Id, currentUser.Id, "AgentAssigned", previous, request.AgentId.ToString()));
        await dbContext.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyAsync(request.AgentId, "TicketAssigned", $"Ticket {ticket.TicketNumber} has been assigned to you.", ticket.Id, cancellationToken);
        return Map(ticket);
    }

    public async Task<TicketCommentDto> AddCommentAsync(Guid ticketId, AddTicketCommentRequest request, CurrentUser currentUser, CancellationToken cancellationToken = default)
    {
        var ticket = await dbContext.Tickets.SingleOrDefaultAsync(x => x.Id == ticketId, cancellationToken) ?? throw new NotFoundException("Ticket not found.");
        EnsureCanView(ticket, currentUser);
        if (currentUser.Role == UserRole.Customer && request.IsInternal) throw new ForbiddenException("Customers cannot add internal comments.");
        if (currentUser.Role == UserRole.Agent && ticket.AssignedAgentId != currentUser.Id) throw new ForbiddenException("Agents can comment only on assigned tickets.");
        if (ticket.Status == TicketStatus.Closed) throw new ValidationException("Closed tickets cannot be modified.");
        if (string.IsNullOrWhiteSpace(request.Content) || request.Content.Length > 8000) throw new ValidationException("Comment content is required and must not exceed 8000 characters.");
        var comment = TicketComment.Create(ticketId, currentUser.Id, request.Content, request.IsInternal);
        dbContext.TicketComments.Add(comment);
        if (currentUser.Role == UserRole.Agent && !request.IsInternal && ticket.RegisterFirstResponse(DateTimeOffset.UtcNow))
            dbContext.TicketHistories.Add(TicketHistory.Create(ticketId, currentUser.Id, "FirstResponseRecorded"));
        dbContext.TicketHistories.Add(TicketHistory.Create(ticketId, currentUser.Id, "CommentAdded", null, request.IsInternal ? "Internal" : "Public"));
        await dbContext.SaveChangesAsync(cancellationToken);
        var recipient = currentUser.Role == UserRole.Customer ? ticket.AssignedAgentId : ticket.CustomerId;
        if (recipient is Guid userId) await notificationService.NotifyAsync(userId, "TicketCommentAdded", $"A new comment was added to ticket {ticket.TicketNumber}.", ticket.Id, cancellationToken);
        return new TicketCommentDto(comment.Id, comment.UserId, comment.Content, comment.IsInternal, comment.CreatedAt);
    }

    private async Task<Ticket> GetForWorkAsync(Guid ticketId, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var ticket = await dbContext.Tickets.SingleOrDefaultAsync(x => x.Id == ticketId, cancellationToken) ?? throw new NotFoundException("Ticket not found.");
        if (currentUser.Role == UserRole.Customer) throw new ForbiddenException("Customers cannot change ticket status.");
        if (currentUser.Role == UserRole.Agent && ticket.AssignedAgentId != currentUser.Id) throw new ForbiddenException("Agents can change only assigned tickets.");
        return ticket;
    }

    private static void EnsureCanView(Ticket ticket, CurrentUser user)
    {
        if (user.Role == UserRole.Customer && ticket.CustomerId != user.Id) throw new ForbiddenException("You do not have access to this ticket.");
        if (user.Role == UserRole.Agent && ticket.AssignedAgentId is not null && ticket.AssignedAgentId != user.Id) throw new ForbiddenException("You do not have access to this ticket.");
    }

    private async Task<string> CreateNumberAsync(CancellationToken cancellationToken)
    {
        string number;
        do { number = $"SD-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(100000, 999999)}"; }
        while (await dbContext.Tickets.AnyAsync(x => x.TicketNumber == number, cancellationToken));
        return number;
    }

    private static TicketDto Map(Ticket ticket, IReadOnlyList<TicketCommentDto>? comments = null, IReadOnlyList<TicketHistoryDto>? history = null) => new(ticket.Id, ticket.TicketNumber, ticket.Title, ticket.Description, ticket.Priority, ticket.Status, ticket.CustomerId, ticket.AssignedAgentId, ticket.CreatedAt, ticket.UpdatedAt, comments, history, ticket.AiPredictedCategory, ticket.AiPredictedPriority, ticket.AiConfidence, ticket.AiReviewRequired, ticket.AiClassificationStatus);
}
