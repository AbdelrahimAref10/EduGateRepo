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

namespace Academy.Application.Features.SuperAdmin.Users.Commands.UpdateAdminUserRole;

public sealed class UpdateAdminUserRoleCommandHandler(
    UserManager<ApplicationUser> userManager,
    IApplicationDbContext dbContext)
    : IRequestHandler<UpdateAdminUserRoleCommand, Result<AdminUserListItemDto>>
{
    public async Task<Result<AdminUserListItemDto>> Handle(
        UpdateAdminUserRoleCommand request,
        CancellationToken cancellationToken)
    {
        if (request.UserId == request.ActingUserId)
            return Result<AdminUserListItemDto>.Failure("You cannot change your own role.");

        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
            return Result<AdminUserListItemDto>.NotFound("User was not found.");

        var currentRoles = await userManager.GetRolesAsync(user);
        var newRoleName = AppRoles.ToRoleName(request.Role);

        if (currentRoles.Count == 1 && currentRoles[0] == newRoleName)
        {
            return Result<AdminUserListItemDto>.Success(await ToDtoAsync(user, currentRoles, cancellationToken));
        }

        var blockReason = await GetRoleChangeBlockReasonAsync(user.Id, cancellationToken);
        if (blockReason is not null)
            return Result<AdminUserListItemDto>.Conflict(blockReason);

        if (currentRoles.Count > 0)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                var error = string.Join(" ", removeResult.Errors.Select(e => e.Description));
                return Result<AdminUserListItemDto>.Failure(error);
            }
        }

        var addResult = await userManager.AddToRoleAsync(user, newRoleName);
        if (!addResult.Succeeded)
        {
            var error = string.Join(" ", addResult.Errors.Select(e => e.Description));
            return Result<AdminUserListItemDto>.Failure(error);
        }

        await RemoveProfilesAsync(user.Id, cancellationToken);
        var studentCode = await AddProfileAsync(user.Id, request.Role, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (request.Role is not AppRole.SuperAdmin)
            await UserPermissionHelper.RemovePermissionAsync(userManager, user, AppPermissions.ManageUsers);

        var roles = await userManager.GetRolesAsync(user);
        var dto = await ToDtoAsync(user, roles, cancellationToken);
        if (studentCode is not null)
        {
            dto = new AdminUserListItemDto
            {
                Id = dto.Id,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                AreaId = dto.AreaId,
                Roles = dto.Roles,
                StudentCode = studentCode,
                HasManageUsers = dto.HasManageUsers,
                CreatedAtUtc = dto.CreatedAtUtc
            };
        }

        return Result<AdminUserListItemDto>.Success(dto);
    }

    private async Task<string?> GetRoleChangeBlockReasonAsync(int userId, CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .AsNoTracking()
            .Include(x => x.Lessons)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (teacher is not null && teacher.Lessons.Count > 0)
            return "Cannot change role: this teacher already has lessons.";

        var student = await dbContext.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (student is not null)
        {
            var hasBookings = await dbContext.LessonBookings
                .AnyAsync(x => x.StudentId == student.Id, cancellationToken);
            if (hasBookings)
                return "Cannot change role: this student already has bookings.";

            var hasMemberships = await dbContext.LessonGroupMembers
                .AnyAsync(x => x.StudentId == student.Id, cancellationToken);
            if (hasMemberships)
                return "Cannot change role: this student is already in a group.";
        }

        return null;
    }

    private async Task RemoveProfilesAsync(int userId, CancellationToken cancellationToken)
    {
        var teachers = await dbContext.Teachers
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
        dbContext.Teachers.RemoveRange(teachers);

        var students = await dbContext.Students
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
        dbContext.Students.RemoveRange(students);

        var admins = await dbContext.SuperAdmins
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
        dbContext.SuperAdmins.RemoveRange(admins);
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

    private async Task<AdminUserListItemDto> ToDtoAsync(
        ApplicationUser user,
        IList<string> roles,
        CancellationToken cancellationToken)
    {
        var studentCode = await dbContext.Students
            .AsNoTracking()
            .Where(x => x.UserId == user.Id && !x.IsParent)
            .Select(x => x.StudentCode)
            .FirstOrDefaultAsync(cancellationToken);

        var hasManageUsers = await UserPermissionHelper.HasPermissionAsync(
            userManager,
            user,
            AppPermissions.ManageUsers);

        return new AdminUserListItemDto
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
        };
    }
}
