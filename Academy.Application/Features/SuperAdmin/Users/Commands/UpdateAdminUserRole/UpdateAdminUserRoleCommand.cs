using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Users.Dtos;
using Academy.Domain.Enums;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Users.Commands.UpdateAdminUserRole;

public sealed record UpdateAdminUserRoleCommand(
    int UserId,
    AppRole Role,
    int ActingUserId) : IRequest<Result<AdminUserListItemDto>>;
