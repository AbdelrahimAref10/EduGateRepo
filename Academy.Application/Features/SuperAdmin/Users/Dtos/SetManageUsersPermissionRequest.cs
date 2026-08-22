namespace Academy.Application.Features.SuperAdmin.Users.Dtos;

public sealed class SetManageUsersPermissionRequest
{
    public required bool Granted { get; init; }
}
