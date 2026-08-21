namespace Academy.Application.Features.Account.Dtos;

public sealed class UserProfileDto
{
    public required int UserId { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string Email { get; init; }

    public string? PhoneNumber { get; init; }

    public string? Bio { get; init; }

    public required IReadOnlyList<string> Roles { get; init; }

    /// <summary>
    /// Preferred language id: 1 = Arabic, 2 = English.
    /// </summary>
    public required int LanguageId { get; init; }

    public string? StudentCode { get; init; }

    public bool? IsParent { get; init; }

    public int? AreaId { get; init; }

    public string? AreaName { get; init; }

    public int? CityId { get; init; }

    public string? CityName { get; init; }

    public int? GovernorateId { get; init; }

    public string? GovernorateName { get; init; }

    public int? CountryId { get; init; }

    public string? CountryName { get; init; }
}
