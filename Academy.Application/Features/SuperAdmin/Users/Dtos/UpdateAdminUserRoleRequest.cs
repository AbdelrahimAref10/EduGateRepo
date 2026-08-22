namespace Academy.Application.Features.SuperAdmin.Users.Dtos;

public sealed class UpdateAdminUserRoleRequest
{
    public required Domain.Enums.AppRole Role { get; init; }
}
