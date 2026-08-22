using Academy.Application.Common.Identity;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Identity;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Auth.Dtos;
using Academy.Domain.Common;
using Academy.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    IApplicationDbContext dbContext) : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    public async Task<Result<AuthResponseDto>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
            return Result<AuthResponseDto>.Failure("Invalid email or password.", 401);

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            await userManager.AccessFailedAsync(user);
            return Result<AuthResponseDto>.Failure("Invalid email or password.", 401);
        }

        if (await userManager.IsLockedOutAsync(user))
            return Result<AuthResponseDto>.Failure("Account is locked.", 401);

        await userManager.ResetAccessFailedCountAsync(user);

        var roles = await userManager.GetRolesAsync(user);
        var permissions = await UserPermissionHelper.GetPermissionsAsync(userManager, user);
        var tokens = tokenService.GenerateTokens(new TokenUserInfo
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Roles = roles.ToList(),
            Permissions = permissions,
            LanguageId = user.PreferredLanguage
        });

        dbContext.RefreshTokens.Add(new Academy.Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            Token = tokenService.HashRefreshToken(tokens.RefreshToken),
            ExpiresAtUtc = tokens.RefreshTokenExpiresAtUtc,
            CreatedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        string? studentCode = null;
        if (roles.Contains(AppRoles.Student))
        {
            studentCode = await dbContext.Students
                .Where(x => x.UserId == user.Id && !x.IsParent)
                .Select(x => x.StudentCode)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            AccessTokenExpiresAtUtc = tokens.AccessTokenExpiresAtUtc,
            RefreshTokenExpiresAtUtc = tokens.RefreshTokenExpiresAtUtc,
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Roles = roles.ToList(),
            Permissions = permissions,
            LanguageId = (int)user.PreferredLanguage,
            StudentCode = studentCode,
            AreaId = user.AreaId
        });
    }
}
