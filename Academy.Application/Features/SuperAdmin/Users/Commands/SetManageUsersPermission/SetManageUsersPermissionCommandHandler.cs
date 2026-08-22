using Academy.Application.Common.Identity;
using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Users.Dtos;
using Academy.Domain.Common;
using Academy.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Academy.Application.Features.SuperAdmin.Users.Commands.SetManageUsersPermission;

public sealed class SetManageUsersPermissionCommandHandler(
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<SetManageUsersPermissionCommand, Result<AdminUserListItemDto>>
{
    public async Task<Result<AdminUserListItemDto>> Handle(
        SetManageUsersPermissionCommand request,
        CancellationToken cancellationToken)
    {
        if (request.UserId == request.ActingUserId && !request.Granted)
            return Result<AdminUserListItemDto>.Failure("You cannot revoke your own ManageUsers permission.");

        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
            return Result<AdminUserListItemDto>.NotFound("User was not found.");

        if (!await userManager.IsInRoleAsync(user, AppRoles.SuperAdmin))
            return Result<AdminUserListItemDto>.Failure("ManageUsers can only be granted to SuperAdmin users.");

        if (request.Granted)
            await UserPermissionHelper.EnsurePermissionAsync(userManager, user, AppPermissions.ManageUsers);
        else
            await UserPermissionHelper.RemovePermissionAsync(userManager, user, AppPermissions.ManageUsers);

        var roles = await userManager.GetRolesAsync(user);
        var hasManageUsers = await UserPermissionHelper.HasPermissionAsync(
            userManager,
            user,
            AppPermissions.ManageUsers);

        return Result<AdminUserListItemDto>.Success(new AdminUserListItemDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            AreaId = user.AreaId,
            Roles = roles.ToList(),
            StudentCode = null,
            HasManageUsers = hasManageUsers,
            CreatedAtUtc = user.CreatedAtUtc
        });
    }
}
