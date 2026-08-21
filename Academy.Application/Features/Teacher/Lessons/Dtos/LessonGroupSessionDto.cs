namespace Academy.Application.Features.Teacher.Lessons.Dtos;

public sealed class LessonGroupSessionDto
{
    public required int Id { get; init; }

    public required int LessonGroupId { get; init; }

    public required DateOnly SessionDate { get; init; }

    public required TimeOnly StartTime { get; init; }

    public string? Topic { get; init; }

    public string? Description { get; init; }

    public DateTime? StartedAtUtc { get; init; }

    public DateTime? EndedAtUtc { get; init; }

    public required bool HasStarted { get; init; }

    public required bool HasEnded { get; init; }

    public required bool CanStart { get; init; }

    /// <summary>True after the session has been started — classroom can be opened.</summary>
    public required bool CanOpenClassroom { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
