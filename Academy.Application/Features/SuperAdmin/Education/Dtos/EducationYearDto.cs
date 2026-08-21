namespace Academy.Application.Features.SuperAdmin.Education.Dtos;

public sealed class EducationYearDto
{
    public required int Id { get; init; }

    public required int EducationStageId { get; init; }

    public required string EducationStageName { get; init; }

    public required int EducationTypeId { get; init; }

    public required string EducationTypeName { get; init; }

    public required string Name { get; init; }

    public required string NameAr { get; init; }

    public required string NameEn { get; init; }

    public required int SortOrder { get; init; }

    public required bool IsActive { get; init; }

    public required int SubjectsCount { get; init; }
}
