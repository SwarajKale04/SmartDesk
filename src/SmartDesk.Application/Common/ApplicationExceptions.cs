namespace SmartDesk.Application.Common;

public sealed class ValidationException(string message) : Exception(message);
public sealed class ConflictException(string message) : Exception(message);
public sealed class UnauthorizedException(string message) : Exception(message);
