using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartDesk.Application.Authentication;
using SmartDesk.Infrastructure.Authentication;
using SmartDesk.Infrastructure.Persistence;
using SmartDesk.Application.Tickets;
using SmartDesk.Infrastructure.Tickets;

namespace SmartDesk.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SmartDesk");
        if (!string.IsNullOrWhiteSpace(connectionString))
            services.AddDbContext<SmartDeskDbContext>(options => options.UseSqlServer(connectionString));
        services.AddOptions<JwtSettings>().Bind(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ITicketService, TicketService>();
        return services;
    }
}
