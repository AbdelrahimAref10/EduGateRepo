namespace Academy.Application.Features.SuperAdmin.Education.Dtos;

public sealed class EducationTypeDto
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required string NameAr { get; init; }

    public required string NameEn { get; init; }

    public required int SortOrder { get; init; }

    public required bool IsActive { get; init; }

    public required int YearsCount { get; init; }
}
