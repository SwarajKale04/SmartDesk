using Serilog;
using SmartDesk.API.Middleware;
using SmartDesk.Application;
using SmartDesk.Infrastructure;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((context, services, configuration) => configuration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services).Enrich.FromLogContext().WriteTo.Console());
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddHealthChecks();
    builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"]).AllowAnyHeader().AllowAnyMethod()));
    var app = builder.Build();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSerilogRequestLogging();
    if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
    app.UseHttpsRedirection();
    app.UseCors("Frontend");
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready");
    app.Run();
}
catch (Microsoft.Extensions.Hosting.HostAbortedException) { }
catch (Exception exception) { Log.Fatal(exception, "SmartDesk API terminated unexpectedly"); }
finally { await Log.CloseAndFlushAsync(); }

public partial class Program;
