using Microsoft.AspNetCore.Mvc;

namespace SmartDesk.API.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception for request {RequestPath}", context.Request.Path);
            var problem = new ProblemDetails { Status = StatusCodes.Status500InternalServerError, Title = "An unexpected error occurred." };
            problem.Extensions["traceId"] = context.TraceIdentifier;
            context.Response.StatusCode = problem.Status.Value;
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
