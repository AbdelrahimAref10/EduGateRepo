namespace Academy.Application.Features.SuperAdmin.Education.Dtos;

public sealed class EducationSubjectDto
{
    public required int Id { get; init; }

    public required int EducationYearId { get; init; }

    public required string EducationYearName { get; init; }

    public required int EducationStageId { get; init; }

    public required string EducationStageName { get; init; }

    public required string Name { get; init; }

    public required string NameAr { get; init; }

    public required string NameEn { get; init; }

    public required int SortOrder { get; init; }

    public required bool IsActive { get; init; }
}
