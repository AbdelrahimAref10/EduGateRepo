namespace Academy.Application.Features.SuperAdmin.Education.Dtos;

public sealed class AcademicYearDto
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required int SortOrder { get; init; }

    public required bool IsActive { get; init; }

    public required int LessonsCount { get; init; }
}

public sealed class CreateAcademicYearRequest
{
    public string Name { get; set; } = null!;

    public int SortOrder { get; set; }
}

public sealed class UpdateAcademicYearRequest
{
    public string Name { get; set; } = null!;

    public int SortOrder { get; set; }
}
