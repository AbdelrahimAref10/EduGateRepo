namespace Academy.Application.Features.Student.Lessons.Dtos;

public sealed class StudentLessonSessionDto
{
    public required int SessionId { get; init; }

    public required int LessonGroupId { get; init; }

    public required DateOnly SessionDate { get; init; }

    public required TimeOnly StartTime { get; init; }

    public string? Topic { get; init; }

    public required bool HasStarted { get; init; }

    public required bool HasEnded { get; init; }

    /// <summary>True after the teacher starts the session.</summary>
    public required bool CanOpenClassroom { get; init; }
}
