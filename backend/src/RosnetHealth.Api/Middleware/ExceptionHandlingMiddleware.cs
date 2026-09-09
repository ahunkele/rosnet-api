using Microsoft.AspNetCore.Mvc;
using RosnetHealth.Application.Exceptions;

namespace RosnetHealth.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DuplicateUrlException ex)
        {
            logger.LogWarning(ex, "Rejected duplicate URL: {Url}", ex.Url);
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Duplicate URL", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                "An unexpected error occurred while processing your request.");
        }
    }

    private static Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };

        return context.Response.WriteAsJsonAsync(problem);
    }
}
