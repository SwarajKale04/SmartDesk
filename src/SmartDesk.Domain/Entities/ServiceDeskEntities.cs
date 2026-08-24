using SmartDesk.Domain.Common;
using SmartDesk.Domain.Enums;

namespace SmartDesk.Domain.Entities;

public sealed class User : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public string? Department { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static User Create(string name, string email, string passwordHash, UserRole role, string? department = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        return new User { Name = name.Trim(), Email = email.Trim().ToLowerInvariant(), PasswordHash = passwordHash, Role = role, Department = department?.Trim() };
    }
}

public sealed class Ticket : Entity
{
    public string TicketNumber { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid? CategoryId { get; private set; }
    public TicketPriority Priority { get; private set; } = TicketPriority.Medium;
    public TicketStatus Status { get; private set; } = TicketStatus.New;
    public Guid CustomerId { get; private set; }
    public Guid? AssignedAgentId { get; private set; }
    public decimal? AiConfidence { get; private set; }
    public string? AiPredictedCategory { get; private set; }
    public TicketPriority? AiPredictedPriority { get; private set; }
    public bool AiReviewRequired { get; private set; }
    public Guid? SlaId { get; private set; }
    public SlaStatus SlaStatus { get; private set; } = SlaStatus.OnTrack;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FirstResponseAt { get; private set; }
    public DateTimeOffset? FirstResponseDueAt { get; private set; }
    public DateTimeOffset? DueAt { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }

    public static Ticket Create(string ticketNumber, string title, string description, Guid customerId, TicketPriority priority = TicketPriority.Medium)
    {
        if (string.IsNullOrWhiteSpace(ticketNumber)) throw new ArgumentException("Ticket number is required.", nameof(ticketNumber));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required.", nameof(description));
        if (customerId == Guid.Empty) throw new ArgumentException("Customer is required.", nameof(customerId));
        return new Ticket { TicketNumber = ticketNumber, Title = title.Trim(), Description = description.Trim(), CustomerId = customerId, Priority = priority };
    }

    public void ChangeStatus(TicketStatus status, DateTimeOffset now)
    {
        if (!IsValidTransition(Status, status)) throw new InvalidOperationException($"Cannot change ticket status from {Status} to {status}.");
        Status = status;
        UpdatedAt = now;
        if (status == TicketStatus.Resolved) { ResolvedAt = now; SlaStatus = SlaStatus.Completed; }
        if (status == TicketStatus.Closed) ClosedAt = now;
        if (status == TicketStatus.Reopened) { ResolvedAt = null; ClosedAt = null; SlaStatus = SlaStatus.OnTrack; }
    }

    public void AssignTo(Guid agentId, DateTimeOffset now)
    {
        if (agentId == Guid.Empty) throw new ArgumentException("Agent is required.", nameof(agentId));
        if (Status == TicketStatus.Closed) throw new InvalidOperationException("Closed tickets cannot be assigned.");
        AssignedAgentId = agentId;
        UpdatedAt = now;
    }

    public void UpdateDetails(string title, string description, TicketPriority priority, DateTimeOffset now)
    {
        if (Status == TicketStatus.Closed) throw new InvalidOperationException("Closed tickets cannot be modified.");
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Title and description are required.");
        Title = title.Trim();
        Description = description.Trim();
        Priority = priority;
        UpdatedAt = now;
    }

    public void ApplySla(Guid slaId, DateTimeOffset firstResponseDueAt, DateTimeOffset dueAt)
    {
        SlaId = slaId;
        FirstResponseDueAt = firstResponseDueAt;
        DueAt = dueAt;
        SlaStatus = SlaStatus.OnTrack;
    }

    public bool RegisterFirstResponse(DateTimeOffset now)
    {
        if (FirstResponseAt is not null) return false;
        FirstResponseAt = now;
        UpdatedAt = now;
        return true;
    }

    public bool UpdateSlaStatus(SlaStatus slaStatus)
    {
        if (SlaStatus == slaStatus) return false;
        SlaStatus = slaStatus;
        return true;
    }

    private static bool IsValidTransition(TicketStatus from, TicketStatus to) => (from, to) switch
    {
        (TicketStatus.New, TicketStatus.Open) => true,
        (TicketStatus.Open, TicketStatus.InProgress) => true,
        (TicketStatus.InProgress, TicketStatus.WaitingForCustomer) => true,
        (TicketStatus.WaitingForCustomer, TicketStatus.InProgress) => true,
        (TicketStatus.InProgress, TicketStatus.Resolved) => true,
        (TicketStatus.Resolved, TicketStatus.Closed) => true,
        (TicketStatus.Resolved, TicketStatus.Reopened) => true,
        (TicketStatus.Closed, TicketStatus.Reopened) => true,
        (TicketStatus.Reopened, TicketStatus.InProgress) => true,
        _ => false
    };
}

public sealed class TicketComment : Entity { public Guid TicketId { get; private set; } public Guid UserId { get; private set; } public string Content { get; private set; } = string.Empty; public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow; public bool IsInternal { get; private set; } public static TicketComment Create(Guid ticketId, Guid userId, string content, bool isInternal) => string.IsNullOrWhiteSpace(content) ? throw new ArgumentException("Comment content is required.", nameof(content)) : new TicketComment { TicketId = ticketId, UserId = userId, Content = content.Trim(), IsInternal = isInternal }; }
public sealed class TicketHistory : Entity { public Guid TicketId { get; private set; } public Guid? UserId { get; private set; } public string Action { get; private set; } = string.Empty; public string? OldValue { get; private set; } public string? NewValue { get; private set; } public DateTimeOffset Timestamp { get; private set; } = DateTimeOffset.UtcNow; public static TicketHistory Create(Guid ticketId, Guid? userId, string action, string? oldValue = null, string? newValue = null) => new() { TicketId = ticketId, UserId = userId, Action = action, OldValue = oldValue, NewValue = newValue }; }
public sealed class Category : Entity { public string Name { get; private set; } = string.Empty; public string? Description { get; private set; } public bool IsActive { get; private set; } = true; }
public sealed class SlaPolicy : Entity { public string Name { get; private set; } = string.Empty; public TicketPriority Priority { get; private set; } public int ResponseTimeMinutes { get; private set; } public int ResolutionTimeMinutes { get; private set; } public bool IsActive { get; private set; } = true; public static SlaPolicy Create(string name, TicketPriority priority, int responseTimeMinutes, int resolutionTimeMinutes) => responseTimeMinutes <= 0 || resolutionTimeMinutes <= 0 ? throw new ArgumentOutOfRangeException(nameof(responseTimeMinutes), "SLA time limits must be positive.") : new SlaPolicy { Name = name, Priority = priority, ResponseTimeMinutes = responseTimeMinutes, ResolutionTimeMinutes = resolutionTimeMinutes }; }
public sealed class Notification : Entity { public Guid UserId { get; private set; } public string Type { get; private set; } = string.Empty; public string Message { get; private set; } = string.Empty; public Guid? RelatedTicketId { get; private set; } public bool IsRead { get; private set; } public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow; public static Notification Create(Guid userId, string type, string message, Guid? relatedTicketId = null) => new() { UserId = userId, Type = type, Message = message, RelatedTicketId = relatedTicketId }; }
