using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Users.Dtos;
using Academy.Domain.Enums;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Users.Commands.CreateAdminUser;

public sealed record CreateAdminUserCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    AppRole Role,
    int? AreaId,
    bool GrantManageUsers = false) : IRequest<Result<AdminUserListItemDto>>;
