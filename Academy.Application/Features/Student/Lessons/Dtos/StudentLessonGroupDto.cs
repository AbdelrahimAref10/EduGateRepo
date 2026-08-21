namespace Academy.Application.Features.Student.Lessons.Dtos;

public sealed class StudentLessonGroupDto
{
    public required int GroupId { get; init; }

    public required string Name { get; init; }

    public required DateOnly PeriodStartDate { get; init; }

    public required DateOnly PeriodEndDate { get; init; }

    public required string AreaName { get; init; }

    public required string Address { get; init; }

    public string? Notes { get; init; }

    public required bool HasStarted { get; init; }

    public required bool HasEnded { get; init; }

    public required IReadOnlyList<StudentLessonSessionDto> Sessions { get; init; }
}
