using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmartDesk.Infrastructure.Persistence;

public sealed class SmartDeskDbContextFactory : IDesignTimeDbContextFactory<SmartDeskDbContext>
{
    public SmartDeskDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SmartDesk")
            ?? "Server=(localdb)\\mssqllocaldb;Database=SmartDesk;Trusted_Connection=True;TrustServerCertificate=True";
        return new SmartDeskDbContext(new DbContextOptionsBuilder<SmartDeskDbContext>().UseSqlServer(connectionString).Options);
    }
}
