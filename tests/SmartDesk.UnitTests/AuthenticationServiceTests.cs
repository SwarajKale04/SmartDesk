using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartDesk.Application.Authentication;
using SmartDesk.Application.Common;
using SmartDesk.Domain.Enums;
using SmartDesk.Infrastructure.Authentication;
using SmartDesk.Infrastructure.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SmartDesk.UnitTests;

public class AuthenticationServiceTests
{
    [Fact]
    public async Task RegisterAsync_WithValidCustomer_ShouldPersistUserAndIssueJwt()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var result = await service.RegisterAsync(new RegisterUserRequest("Ada Lovelace", "ADA@EXAMPLE.TEST", "a-secure-password", "Engineering"));

        Assert.Equal(UserRole.Customer, result.Role);
        Assert.Equal("ada@example.test", result.Email);
        Assert.NotEmpty(result.AccessToken);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
        Assert.Equal(result.UserId.ToString(), token.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Single(dbContext.Users);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ShouldRejectDuplicateAccount()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        await service.RegisterAsync(new RegisterUserRequest("Ada", "ada@example.test", "a-secure-password", null));

        await Assert.ThrowsAsync<ConflictException>(() => service.RegisterAsync(new RegisterUserRequest("Other Ada", "ADA@example.test", "another-secure-password", null)));
    }

    private static SmartDeskDbContext CreateDbContext() => new(new DbContextOptionsBuilder<SmartDeskDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static AuthenticationService CreateService(SmartDeskDbContext dbContext) => new(dbContext, Options.Create(new JwtSettings
    {
        Issuer = "SmartDesk.Tests", Audience = "SmartDesk.Tests", SigningKey = "test-signing-key-with-at-least-thirty-two-characters", ExpiryMinutes = 60
    }));
}
