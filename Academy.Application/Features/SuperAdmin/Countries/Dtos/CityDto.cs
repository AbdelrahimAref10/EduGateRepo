namespace Academy.Application.Features.SuperAdmin.Countries.Dtos;

public sealed class CityDto
{
    public required int Id { get; init; }

    public required int GovernorateId { get; init; }

    public required string Name { get; init; }

    public required string NameAr { get; init; }

    public required string NameEn { get; init; }

    public required bool IsActive { get; init; }

    public required int AreasCount { get; init; }
}
