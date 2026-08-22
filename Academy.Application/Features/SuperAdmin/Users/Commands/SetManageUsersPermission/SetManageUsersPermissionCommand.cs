using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Users.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Users.Commands.SetManageUsersPermission;

public sealed record SetManageUsersPermissionCommand(
    int UserId,
    bool Granted,
    int ActingUserId) : IRequest<Result<AdminUserListItemDto>>;
