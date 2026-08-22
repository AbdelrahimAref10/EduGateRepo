using System.Security.Claims;
using Academy.Domain.Common;
using Academy.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Academy.Application.Common.Identity;

public static class UserPermissionHelper
{
    public static async Task<IReadOnlyList<string>> GetPermissionsAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user)
    {
        var claims = await userManager.GetClaimsAsync(user);
        return claims
            .Where(c => c.Type == AppPermissions.ClaimType)
            .Select(c => c.Value)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    public static async Task<bool> HasPermissionAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user,
        string permission)
    {
        var permissions = await GetPermissionsAsync(userManager, user);
        return permissions.Contains(permission, StringComparer.Ordinal);
    }

    public static async Task EnsurePermissionAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user,
        string permission)
    {
        if (await HasPermissionAsync(userManager, user, permission))
            return;

        var result = await userManager.AddClaimAsync(
            user,
            new Claim(AppPermissions.ClaimType, permission));

        if (!result.Succeeded)
        {
            var error = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to grant permission '{permission}': {error}");
        }
    }

    public static async Task RemovePermissionAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user,
        string permission)
    {
        var claims = await userManager.GetClaimsAsync(user);
        var matches = claims
            .Where(c => c.Type == AppPermissions.ClaimType && c.Value == permission)
            .ToList();

        foreach (var claim in matches)
        {
            var result = await userManager.RemoveClaimAsync(user, claim);
            if (!result.Succeeded)
            {
                var error = string.Join(" ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to revoke permission '{permission}': {error}");
            }
        }
    }
}
