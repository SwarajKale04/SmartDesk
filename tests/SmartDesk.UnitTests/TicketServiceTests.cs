using Microsoft.EntityFrameworkCore;
using SmartDesk.Application.Common;
using SmartDesk.Application.Tickets;
using SmartDesk.Domain.Entities;
using SmartDesk.Domain.Enums;
using SmartDesk.Infrastructure.Persistence;
using SmartDesk.Infrastructure.Tickets;

namespace SmartDesk.UnitTests;

public class TicketServiceTests
{
    [Fact]
    public async Task CreateAsync_AsCustomer_ShouldCreateNewTicketAndHistory()
    {
        await using var dbContext = CreateDbContext();
        var service = new TicketService(dbContext);
        var customer = new CurrentUser(Guid.NewGuid(), UserRole.Customer);

        var result = await service.CreateAsync(new CreateTicketRequest("Cannot connect to VPN", "The VPN disconnects every five minutes.", TicketPriority.High), customer);

        Assert.StartsWith("SD-", result.TicketNumber);
        Assert.Equal(TicketStatus.New, result.Status);
        Assert.Equal(TicketPriority.High, result.Priority);
        Assert.Single(dbContext.TicketHistories);
    }

    [Fact]
    public async Task ChangeStatusAsync_WithInvalidTransition_ShouldReturnValidationError()
    {
        await using var dbContext = CreateDbContext();
        var service = new TicketService(dbContext);
        var customer = new CurrentUser(Guid.NewGuid(), UserRole.Customer);
        var ticket = await service.CreateAsync(new CreateTicketRequest("Laptop issue", "The laptop will not start."), customer);
        var agent = User.Create("Agent", "agent@example.test", "hash", UserRole.Agent);
        dbContext.Users.Add(agent);
        await dbContext.SaveChangesAsync();
        await service.AssignAsync(ticket.Id, new AssignTicketRequest(agent.Id), new CurrentUser(Guid.NewGuid(), UserRole.Admin));

        await Assert.ThrowsAsync<ValidationException>(() => service.ChangeStatusAsync(ticket.Id, new UpdateTicketStatusRequest(TicketStatus.Resolved), new CurrentUser(agent.Id, UserRole.Agent)));
    }

    [Fact]
    public async Task GetByIdAsync_AsDifferentCustomer_ShouldDenyAccess()
    {
        await using var dbContext = CreateDbContext();
        var service = new TicketService(dbContext);
        var owner = new CurrentUser(Guid.NewGuid(), UserRole.Customer);
        var ticket = await service.CreateAsync(new CreateTicketRequest("Email issue", "Outlook is not opening."), owner);

        await Assert.ThrowsAsync<ForbiddenException>(() => service.GetByIdAsync(ticket.Id, new CurrentUser(Guid.NewGuid(), UserRole.Customer)));
    }

    private static SmartDeskDbContext CreateDbContext() => new(new DbContextOptionsBuilder<SmartDeskDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
}
