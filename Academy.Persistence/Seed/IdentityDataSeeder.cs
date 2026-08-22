using Academy.Domain.Common;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using Academy.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Academy.Persistence.Seed;

public static class IdentityDataSeeder
{
    public const string DefaultEmail = "admin@edugate.com";
    public const string DefaultPassword = "Admin@123456";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("IdentityDataSeeder");
        var configuration = sp.GetRequiredService<IConfiguration>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var db = sp.GetRequiredService<AcademyDbContext>();

        var section = configuration.GetSection("Seed:SuperAdmin");
        var email = (section["Email"] ?? DefaultEmail).Trim();
        var password = section["Password"] ?? DefaultPassword;
        var firstName = (section["FirstName"] ?? "Super").Trim();
        var lastName = (section["LastName"] ?? "Admin").Trim();

        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            if (!await userManager.IsInRoleAsync(existing, AppRoles.SuperAdmin))
                await userManager.AddToRoleAsync(existing, AppRoles.SuperAdmin);

            await EnsureManageUsersPermissionAsync(userManager, existing);

            var hasProfile = await db.SuperAdmins.AnyAsync(x => x.UserId == existing.Id);
            if (!hasProfile)
            {
                db.SuperAdmins.Add(new SuperAdmin
                {
                    UserId = existing.Id,
                    CreatedAtUtc = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }

            logger.LogInformation("SuperAdmin seed skipped — user already exists ({Email}).", email);
            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            AreaId = null,
            PreferredLanguage = AppLanguage.Arabic,
            EmailConfirmed = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var error = string.Join("; ", createResult.Errors.Select(e => e.Description));
            logger.LogError("Failed to seed SuperAdmin: {Error}", error);
            throw new InvalidOperationException($"Failed to seed SuperAdmin: {error}");
        }

        var roleResult = await userManager.AddToRoleAsync(user, AppRoles.SuperAdmin);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            var error = string.Join("; ", roleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to assign SuperAdmin role: {error}");
        }

        db.SuperAdmins.Add(new SuperAdmin
        {
            UserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        await EnsureManageUsersPermissionAsync(userManager, user);

        logger.LogInformation("SuperAdmin seeded successfully ({Email}).", email);
    }

    private static async Task EnsureManageUsersPermissionAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user)
    {
        var claims = await userManager.GetClaimsAsync(user);
        var hasPermission = claims.Any(c =>
            c.Type == AppPermissions.ClaimType && c.Value == AppPermissions.ManageUsers);

        if (hasPermission)
            return;

        var result = await userManager.AddClaimAsync(
            user,
            new System.Security.Claims.Claim(AppPermissions.ClaimType, AppPermissions.ManageUsers));

        if (!result.Succeeded)
        {
            var error = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to grant ManageUsers permission: {error}");
        }
    }
}
