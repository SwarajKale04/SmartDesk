using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SmartDesk.API.Middleware;
using SmartDesk.Application;
using SmartDesk.Infrastructure;
using SmartDesk.Infrastructure.Persistence;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((context, services, configuration) => configuration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services).Enrich.FromLogContext().WriteTo.Console());
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT", Description = "Paste a JWT access token."
        });
        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            [new Microsoft.OpenApi.Models.OpenApiSecurityScheme { Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } }] = Array.Empty<string>()
        });
    });
    builder.Services.AddHealthChecks();
    builder.Services.AddSignalR();
    var jwtSection = builder.Configuration.GetSection("Jwt");
    var jwtSigningKey = jwtSection["SigningKey"];
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidIssuer = jwtSection["Issuer"], ValidateAudience = true, ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey ?? string.Empty)), ValidateLifetime = true
        });
    builder.Services.AddAuthorization();
    builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"]).AllowAnyHeader().AllowAnyMethod()));
    var app = builder.Build();
    if (app.Environment.IsDevelopment() && !string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("SmartDesk")))
    {
        using var scope = app.Services.CreateScope();
        await DevelopmentDataSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<SmartDeskDbContext>(), builder.Configuration);
    }
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSerilogRequestLogging();
    if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
    app.UseHttpsRedirection();
    app.UseCors("Frontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready");
    app.MapHub<SmartDesk.Infrastructure.Notifications.NotificationHub>("/hubs/notifications");
    app.Run();
}
catch (Microsoft.Extensions.Hosting.HostAbortedException) { }
catch (Exception exception) { Log.Fatal(exception, "SmartDesk API terminated unexpectedly"); }
finally { await Log.CloseAndFlushAsync(); }

public partial class Program;
