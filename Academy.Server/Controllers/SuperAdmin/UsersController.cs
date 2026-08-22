using System.Security.Claims;
using Academy.Application.Features.SuperAdmin.Users.Commands.CreateAdminUser;
using Academy.Application.Features.SuperAdmin.Users.Commands.SetManageUsersPermission;
using Academy.Application.Features.SuperAdmin.Users.Commands.UpdateAdminUserRole;
using Academy.Application.Features.SuperAdmin.Users.Dtos;
using Academy.Application.Features.SuperAdmin.Users.Queries.GetAdminUsers;
using Academy.Domain.Common;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.SuperAdmin;

[ApiController]
[Authorize(Policy = AppPolicies.ManageUsers)]
[Route("api/super-admin/users")]
[Produces("application/json")]
public sealed class UsersController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AdminUserListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminUsersQuery(), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    [ProducesResponseType(typeof(AdminUserListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateAdminUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateAdminUserCommand(
                request.Email,
                request.Password,
                request.ConfirmPassword,
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                request.Role,
                request.AreaId,
                request.GrantManageUsers),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut("{userId:int}/role")]
    [ProducesResponseType(typeof(AdminUserListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateUserRole(
        int userId,
        [FromBody] UpdateAdminUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        var actingUserId = GetUserId();
        if (actingUserId is null)
            return Unauthorized();

        var result = await sender.Send(
            new UpdateAdminUserRoleCommand(userId, request.Role, actingUserId.Value),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut("{userId:int}/permissions/manage-users")]
    [ProducesResponseType(typeof(AdminUserListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetManageUsersPermission(
        int userId,
        [FromBody] SetManageUsersPermissionRequest request,
        CancellationToken cancellationToken)
    {
        var actingUserId = GetUserId();
        if (actingUserId is null)
            return Unauthorized();

        var result = await sender.Send(
            new SetManageUsersPermissionCommand(userId, request.Granted, actingUserId.Value),
            cancellationToken);

        return result.ToActionResult();
    }

    private int? GetUserId()
    {
        var raw =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return int.TryParse(raw, out var id) && id > 0 ? id : null;
    }
}
