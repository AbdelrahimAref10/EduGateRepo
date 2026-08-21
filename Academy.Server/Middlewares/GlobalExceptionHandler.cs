using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Middlewares;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, errors) = MapException(exception);

        if (statusCode >= StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception occurred.");
        else
            _logger.LogWarning(exception, "Handled exception: {Title}", title);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = GetDetail(exception, statusCode),
            Instance = httpContext.Request.Path,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        if (errors is not null)
            problem.Extensions["errors"] = errors;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }

    private string? GetDetail(Exception exception, int statusCode)
    {
        if (statusCode >= StatusCodes.Status500InternalServerError && !_environment.IsDevelopment())
            return null;

        return exception.Message;
    }

    private static (int StatusCode, string Title, IDictionary<string, string[]>? Errors) MapException(
        Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "One or more validation errors occurred.",
                validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),

            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                null)
        };
    }
}
