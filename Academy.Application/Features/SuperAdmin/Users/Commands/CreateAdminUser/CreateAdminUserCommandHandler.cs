using Academy.Application.Common.Helpers;
using Academy.Application.Common.Identity;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Users.Dtos;
using Academy.Domain.Common;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Users.Commands.CreateAdminUser;

public sealed class CreateAdminUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    IApplicationDbContext dbContext)
    : IRequestHandler<CreateAdminUserCommand, Result<AdminUserListItemDto>>
{
    public async Task<Result<AdminUserListItemDto>> Handle(
        CreateAdminUserCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();

        if (await userManager.FindByEmailAsync(email) is not null)
            return Result<AdminUserListItemDto>.Conflict("Email is already registered.");

        int? areaId = request.AreaId;
        if (request.Role is not AppRole.SuperAdmin)
        {
            if (areaId is null or <= 0)
                return Result<AdminUserListItemDto>.Failure("Area is required for this role.");

            var area = await dbContext.Areas
                .FirstOrDefaultAsync(x => x.Id == areaId && x.IsActive, cancellationToken);

            if (area is null)
                return Result<AdminUserListItemDto>.Failure("Selected area was not found or is inactive.");
        }
        else
        {
            areaId = null;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
                ? null
                : request.PhoneNumber.Trim(),
            AreaId = areaId,
            PreferredLanguage = AppLanguage.Arabic,
            EmailConfirmed = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var error = string.Join(" ", createResult.Errors.Select(e => e.Description));
            return Result<AdminUserListItemDto>.Failure(error);
        }

        var roleName = AppRoles.ToRoleName(request.Role);
        var roleResult = await userManager.AddToRoleAsync(user, roleName);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            var error = string.Join(" ", roleResult.Errors.Select(e => e.Description));
            return Result<AdminUserListItemDto>.Failure(error);
        }

        string? studentCode;
        try
        {
            studentCode = await AddProfileAsync(user.Id, request.Role, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            if (request.Role is AppRole.SuperAdmin && request.GrantManageUsers)
                await UserPermissionHelper.EnsurePermissionAsync(userManager, user, AppPermissions.ManageUsers);
        }
        catch (Exception)
        {
            await userManager.DeleteAsync(user);
            throw;
        }

        var roles = await userManager.GetRolesAsync(user);
        var hasManageUsers = await UserPermissionHelper.HasPermissionAsync(
            userManager,
            user,
            AppPermissions.ManageUsers);

        return Result<AdminUserListItemDto>.Success(new AdminUserListItemDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            AreaId = user.AreaId,
            Roles = roles.ToList(),
            StudentCode = studentCode,
            HasManageUsers = hasManageUsers,
            CreatedAtUtc = user.CreatedAtUtc
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

            case AppRole.SuperAdmin:
                dbContext.SuperAdmins.Add(new Domain.Entities.SuperAdmin
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
