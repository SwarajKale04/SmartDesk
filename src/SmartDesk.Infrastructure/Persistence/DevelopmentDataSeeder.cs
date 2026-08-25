using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartDesk.Domain.Entities;
using SmartDesk.Domain.Enums;

namespace SmartDesk.Infrastructure.Persistence;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(SmartDeskDbContext dbContext, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var password = configuration["Seed:DefaultPassword"];
        await dbContext.Database.MigrateAsync(cancellationToken);
        if (!await dbContext.Categories.AnyAsync(cancellationToken))
        {
            dbContext.Categories.AddRange(new[] { "Hardware", "Software", "Network", "Account Access", "Security", "Email", "Infrastructure", "Other" }.Select(name => Category.Create(name)));
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        if (!await dbContext.SlaPolicies.AnyAsync(cancellationToken))
        {
            dbContext.SlaPolicies.AddRange(
                SlaPolicy.Create("Critical", TicketPriority.Critical, 15, 240), SlaPolicy.Create("High", TicketPriority.High, 30, 480),
                SlaPolicy.Create("Medium", TicketPriority.Medium, 120, 1440), SlaPolicy.Create("Low", TicketPriority.Low, 480, 4320));
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        if (string.IsNullOrWhiteSpace(password)) return;
        if (await dbContext.Users.AnyAsync(cancellationToken)) return;
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        dbContext.Users.AddRange(
            User.Create("SmartDesk Admin", "admin@smartdesk.local", hash, UserRole.Admin, "IT"),
            User.Create("Support Agent", "agent@smartdesk.local", hash, UserRole.Agent, "IT Support"),
            User.Create("Demo Customer", "customer@smartdesk.local", hash, UserRole.Customer, "Operations"));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
