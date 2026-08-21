namespace Academy.Application.Features.SuperAdmin.Countries.Dtos;

public sealed class AreaDto
{
    public required int Id { get; init; }

    public required int CityId { get; init; }

    public required string Name { get; init; }

    public required string NameAr { get; init; }

    public required string NameEn { get; init; }

    public required bool IsActive { get; init; }
}
