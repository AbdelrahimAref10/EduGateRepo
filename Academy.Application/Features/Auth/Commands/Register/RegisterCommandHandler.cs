using Academy.Application.Common.Helpers;
using Academy.Application.Common.Identity;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Identity;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Auth.Dtos;
using Academy.Domain.Common;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    IApplicationDbContext dbContext) : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
{
    public async Task<Result<AuthResponseDto>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Role is AppRole.SuperAdmin)
            return Result<AuthResponseDto>.Failure("SuperAdmin cannot be registered.");

        var email = request.Email.Trim();

        if (await userManager.FindByEmailAsync(email) is not null)
            return Result<AuthResponseDto>.Conflict("Email is already registered.");

        var area = await dbContext.Areas
            .FirstOrDefaultAsync(x => x.Id == request.AreaId && x.IsActive, cancellationToken);

        if (area is null)
            return Result<AuthResponseDto>.Failure("Selected area was not found or is inactive.");

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
                ? null
                : request.PhoneNumber.Trim(),
            AreaId = request.AreaId,
            PreferredLanguage = AppLanguage.Arabic,
            EmailConfirmed = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var error = string.Join(" ", createResult.Errors.Select(e => e.Description));
            return Result<AuthResponseDto>.Failure(error);
        }

        var roleName = AppRoles.ToRoleName(request.Role);
        var roleResult = await userManager.AddToRoleAsync(user, roleName);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            var error = string.Join(" ", roleResult.Errors.Select(e => e.Description));
            return Result<AuthResponseDto>.Failure(error);
        }

        var studentCode = await AddProfileAsync(user.Id, request.Role, cancellationToken);

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

    private async Task<string?> AddProfileAsync(
        int userId,
        AppRole role,
        CancellationToken cancellationToken)
    {
        switch (role)
        {
            case AppRole.Student:
            {
                var code = await StudentCodeGenerator.GenerateUniqueAsync(dbContext, cancellationToken);
                dbContext.Students.Add(new Domain.Entities.Student
                {
                    UserId = userId,
                    IsParent = false,
                    StudentCode = code,
                    CreatedAtUtc = DateTime.UtcNow
                });
                return code;
            }

            case AppRole.Parent:
                dbContext.Students.Add(new Domain.Entities.Student
                {
                    UserId = userId,
                    IsParent = true,
                    StudentCode = null,
                    CreatedAtUtc = DateTime.UtcNow
                });
                return null;

            case AppRole.Teacher:
                dbContext.Teachers.Add(new Domain.Entities.Teacher
                {
                    UserId = userId,
                    CreatedAtUtc = DateTime.UtcNow
                });
                return null;

            default:
                throw new ArgumentOutOfRangeException(nameof(role), role, null);
        }
    }
}
