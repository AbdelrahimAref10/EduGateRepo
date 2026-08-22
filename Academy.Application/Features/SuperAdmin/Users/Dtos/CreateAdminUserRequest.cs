namespace Academy.Application.Features.SuperAdmin.Users.Dtos;

public sealed class CreateAdminUserRequest
{
    public required string Email { get; init; }

    public required string Password { get; init; }

    public required string ConfirmPassword { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public string? PhoneNumber { get; init; }

    public required Domain.Enums.AppRole Role { get; init; }

    public int? AreaId { get; init; }

    /// <summary>Only applies when Role is SuperAdmin.</summary>
    public bool GrantManageUsers { get; init; }
}
