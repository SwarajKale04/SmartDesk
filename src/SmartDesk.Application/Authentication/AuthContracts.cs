using SmartDesk.Domain.Enums;

namespace SmartDesk.Application.Authentication;

public sealed record RegisterUserRequest(string Name, string Email, string Password, string? Department);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResponse(Guid UserId, string Name, string Email, UserRole Role, string AccessToken, DateTimeOffset ExpiresAt);

public interface IAuthenticationService
{
    Task<AuthResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
