using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Users.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Users.Queries.GetAdminUsers;

public sealed record GetAdminUsersQuery : IRequest<Result<IReadOnlyList<AdminUserListItemDto>>>;
