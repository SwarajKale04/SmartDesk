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
    public DateTimeOffset? DueAt { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }
}

public sealed class TicketComment : Entity { public Guid TicketId { get; private set; } public Guid UserId { get; private set; } public string Content { get; private set; } = string.Empty; public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow; public bool IsInternal { get; private set; } }
public sealed class TicketHistory : Entity { public Guid TicketId { get; private set; } public Guid? UserId { get; private set; } public string Action { get; private set; } = string.Empty; public string? OldValue { get; private set; } public string? NewValue { get; private set; } public DateTimeOffset Timestamp { get; private set; } = DateTimeOffset.UtcNow; }
public sealed class Category : Entity { public string Name { get; private set; } = string.Empty; public string? Description { get; private set; } public bool IsActive { get; private set; } = true; }
public sealed class SlaPolicy : Entity { public string Name { get; private set; } = string.Empty; public TicketPriority Priority { get; private set; } public int ResponseTimeMinutes { get; private set; } public int ResolutionTimeMinutes { get; private set; } public bool IsActive { get; private set; } = true; }
public sealed class Notification : Entity { public Guid UserId { get; private set; } public string Type { get; private set; } = string.Empty; public string Message { get; private set; } = string.Empty; public Guid? RelatedTicketId { get; private set; } public bool IsRead { get; private set; } public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow; }
