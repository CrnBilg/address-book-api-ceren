using AddressBook.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AddressBook.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Resource not found", exception.Message);
        }
        catch (ConflictException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Resource conflict", exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception occurred.");
            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Unexpected server error",
                "An unexpected error occurred while processing the request.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problem, (JsonSerializerOptions?)null, "application/problem+json");
    }
}
