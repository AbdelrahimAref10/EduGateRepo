namespace Academy.Application.Features.SuperAdmin.Education.Dtos;

public sealed class EducationStageDto
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required string NameAr { get; init; }

    public required string NameEn { get; init; }

    public required int SortOrder { get; init; }

    public required bool IsActive { get; init; }

    public required int YearsCount { get; init; }
}

public sealed class CreateEducationStageRequest
{
    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public int SortOrder { get; set; }
}

public sealed class UpdateEducationStageRequest
{
    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public int SortOrder { get; set; }
}
