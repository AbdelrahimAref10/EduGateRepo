namespace Academy.Application.Features.SuperAdmin.Users.Dtos;

public sealed class AdminUserListItemDto
{
    public required int Id { get; init; }

    public required string Email { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string FullName { get; init; }

    public string? PhoneNumber { get; init; }

    public int? AreaId { get; init; }

    public required IReadOnlyList<string> Roles { get; init; }

    public string? StudentCode { get; init; }

    public required bool HasManageUsers { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
