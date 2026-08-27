namespace Academy.Application.Features.Teacher.Lessons.Dtos;

public sealed class TeacherLessonCountsDto
{
    public required int Total { get; init; }

    public required IReadOnlyList<TeacherLessonAcademicYearCountDto> ByAcademicYear { get; init; }
}

public sealed class TeacherLessonAcademicYearCountDto
{
    public required int AcademicYearId { get; init; }

    public required string AcademicYearName { get; init; }

    public required int Count { get; init; }
}
