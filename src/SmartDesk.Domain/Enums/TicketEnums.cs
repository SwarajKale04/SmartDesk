namespace SmartDesk.Domain.Enums;

public enum UserRole { Customer, Agent, Admin }
public enum TicketPriority { Low, Medium, High, Critical }
public enum TicketStatus { New, Open, InProgress, WaitingForCustomer, Resolved, Closed, Reopened }
public enum SlaStatus { OnTrack, AtRisk, Breached, Completed }
