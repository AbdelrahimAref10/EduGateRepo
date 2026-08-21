using Academy.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        return ToProblemResult(result);
    }

    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new NoContentResult();

        return ToProblemResult(result);
    }

    public static IActionResult ToCreatedResult<T>(
        this Result<T> result,
        string actionName,
        Func<T, object> routeValues)
    {
        if (!result.IsSuccess)
            return result.ToActionResult();

        return new CreatedAtActionResult(
            actionName,
            null,
            routeValues(result.Value!),
            result.Value);
    }

    private static ObjectResult ToProblemResult(Result result) =>
        new(new ProblemDetails
        {
            Title = GetTitle(result.StatusCode),
            Detail = result.Error,
            Status = result.StatusCode,
            Type = $"https://httpstatuses.com/{result.StatusCode}"
        })
        {
            StatusCode = result.StatusCode
        };

    private static string GetTitle(int statusCode) => statusCode switch
    {
        401 => "Unauthorized",
        404 => "Not Found",
        409 => "Conflict",
        _ => "Bad Request"
    };
}
