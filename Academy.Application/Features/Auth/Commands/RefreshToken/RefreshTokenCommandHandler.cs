using Academy.Application.Common.Models;
using Academy.Application.Contracts.Identity;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Auth.Dtos;
using Academy.Domain.Common;
using Academy.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RefreshTokenEntity = Academy.Domain.Entities.RefreshToken;

namespace Academy.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    IApplicationDbContext dbContext) : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    public async Task<Result<AuthResponseDto>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var hashedToken = tokenService.HashRefreshToken(request.RefreshToken);

        var storedToken = await dbContext.RefreshTokens
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Token == hashedToken, cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
            return Result<AuthResponseDto>.Failure("Invalid or expired refresh token.", 401);

        var user = await userManager.FindByIdAsync(storedToken.UserId.ToString());
        if (user is null)
            return Result<AuthResponseDto>.NotFound("User was not found.");

        storedToken.RevokedAtUtc = DateTime.UtcNow;

        var roles = await userManager.GetRolesAsync(user);
        var tokens = tokenService.GenerateTokens(new TokenUserInfo
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Roles = roles.ToList(),
            LanguageId = user.PreferredLanguage
        });

        dbContext.RefreshTokens.Add(new RefreshTokenEntity
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
            LanguageId = (int)user.PreferredLanguage,
            StudentCode = studentCode,
            AreaId = user.AreaId
        });
    }
}
