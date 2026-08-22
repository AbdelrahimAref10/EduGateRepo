using Academy.Application.Common.Identity;
using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Common.Images;
using Academy.Application.Contracts.Identity;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Auth.Dtos;
using Academy.Domain.Common;
using Academy.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Account.Commands.UpdatePreferredLanguage;

public sealed class UpdatePreferredLanguageCommandHandler(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<UpdatePreferredLanguageCommand, Result<AuthResponseDto>>
{
    public async Task<Result<AuthResponseDto>> Handle(
        UpdatePreferredLanguageCommand request,
        CancellationToken cancellationToken)
    {
        if (!LocalizedNames.TryParse(request.LanguageId, out var language))
            return Result<AuthResponseDto>.Failure("Unsupported language. Use 1 (Arabic) or 2 (English).");

        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
            return Result<AuthResponseDto>.NotFound("User was not found.");

        user.PreferredLanguage = language;
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var error = string.Join(" ", updateResult.Errors.Select(e => e.Description));
            return Result<AuthResponseDto>.Failure(error);
        }

        requestLanguage.Set(language);

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

        dbContext.RefreshTokens.Add(new RefreshToken
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
            AreaId = user.AreaId,
            PhotoUrl = ImageService.DisplayValue(user.ProfilePhoto)
        });
    }
}
