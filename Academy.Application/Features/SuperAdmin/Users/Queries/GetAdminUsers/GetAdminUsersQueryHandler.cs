using Academy.Application.Common.Identity;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Users.Dtos;
using Academy.Domain.Common;
using Academy.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Users.Queries.GetAdminUsers;

public sealed class GetAdminUsersQueryHandler(
    UserManager<ApplicationUser> userManager,
    IApplicationDbContext dbContext)
    : IRequestHandler<GetAdminUsersQuery, Result<IReadOnlyList<AdminUserListItemDto>>>
{
    public async Task<Result<IReadOnlyList<AdminUserListItemDto>>> Handle(
        GetAdminUsersQuery request,
        CancellationToken cancellationToken)
    {
        var users = await userManager.Users
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var studentCodes = await dbContext.Students
            .AsNoTracking()
            .Where(x => !x.IsParent && x.StudentCode != null)
            .ToDictionaryAsync(x => x.UserId, x => x.StudentCode!, cancellationToken);

        var items = new List<AdminUserListItemDto>(users.Count);
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            studentCodes.TryGetValue(user.Id, out var studentCode);
            var hasManageUsers = await UserPermissionHelper.HasPermissionAsync(
                userManager,
                user,
                AppPermissions.ManageUsers);

            items.Add(new AdminUserListItemDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
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

        return Result<IReadOnlyList<AdminUserListItemDto>>.Success(items);
    }
}
