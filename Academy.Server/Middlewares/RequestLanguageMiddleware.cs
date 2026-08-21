using System.Security.Claims;
using Academy.Application.Contracts.Localization;
using Academy.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Academy.Server.Middlewares;

/// <summary>
/// Loads PreferredLanguage from the authenticated user's row (session = JWT identity).
/// </summary>
public sealed class RequestLanguageMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IRequestLanguage requestLanguage,
        UserManager<ApplicationUser> userManager)
    {
        var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdValue, out var userId))
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is not null)
                requestLanguage.Set(user.PreferredLanguage);
        } 

        await next(context);
    }
}
