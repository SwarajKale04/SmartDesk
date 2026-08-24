using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartDesk.Application.Authentication;
using SmartDesk.Infrastructure.Authentication;
using SmartDesk.Infrastructure.Persistence;
using SmartDesk.Application.Tickets;
using SmartDesk.Infrastructure.Tickets;
using SmartDesk.Application.Sla;
using SmartDesk.Infrastructure.Sla;

namespace SmartDesk.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SmartDesk");
        var effectiveConnectionString = connectionString ?? "Server=(localdb)\\mssqllocaldb;Database=SmartDesk;Trusted_Connection=True;TrustServerCertificate=True";
        services.AddDbContext<SmartDeskDbContext>(options => options.UseSqlServer(effectiveConnectionString));
        services.AddOptions<JwtSettings>().Bind(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ISlaCalculationService, SlaCalculationService>();
        services.AddOptions<SlaMonitoringOptions>().Bind(configuration.GetSection(SlaMonitoringOptions.SectionName));
        if (!string.IsNullOrWhiteSpace(connectionString)) services.AddHostedService<SlaMonitoringService>();
        return services;
    }
}
