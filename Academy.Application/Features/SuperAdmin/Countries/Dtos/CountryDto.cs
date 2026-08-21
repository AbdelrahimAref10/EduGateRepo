namespace Academy.Application.Features.SuperAdmin.Countries.Dtos;

public sealed class CountryDto
{
    public required int Id { get; init; }

    /// <summary>Localized display name based on request language.</summary>
    public required string Name { get; init; }

    public required string NameAr { get; init; }

    public required string NameEn { get; init; }

    public required string Code { get; init; }

    public required bool IsActive { get; init; }

    public required int GovernoratesCount { get; init; }
}
