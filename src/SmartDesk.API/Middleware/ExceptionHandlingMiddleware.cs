using Microsoft.AspNetCore.Mvc;
using SmartDesk.Application.Common;

namespace SmartDesk.API.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception for request {RequestPath}", context.Request.Path);
            var status = exception switch { ValidationException => StatusCodes.Status400BadRequest, ConflictException => StatusCodes.Status409Conflict, UnauthorizedException => StatusCodes.Status401Unauthorized, _ => StatusCodes.Status500InternalServerError };
            var title = status == StatusCodes.Status500InternalServerError ? "An unexpected error occurred." : exception.Message;
            var problem = new ProblemDetails { Status = status, Title = title };
            problem.Extensions["traceId"] = context.TraceIdentifier;
            context.Response.StatusCode = problem.Status.Value;
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
