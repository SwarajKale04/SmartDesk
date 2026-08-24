namespace SmartDesk.Infrastructure.Authentication;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";
    public string Issuer { get; init; } = "SmartDesk";
    public string Audience { get; init; } = "SmartDesk.Web";
    public string SigningKey { get; init; } = string.Empty;
    public int ExpiryMinutes { get; init; } = 60;
}
