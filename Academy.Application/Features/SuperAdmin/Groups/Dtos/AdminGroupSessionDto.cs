namespace Academy.Application.Features.SuperAdmin.Groups.Dtos;

public sealed class AdminGroupSessionDto
{
    public required int Id { get; init; }

    public required int LessonGroupId { get; init; }

    public required int SessionNumber { get; init; }

    public required DateOnly SessionDate { get; init; }

    public required TimeOnly StartTime { get; init; }

    public string? Topic { get; init; }

    public DateTime? StartedAtUtc { get; init; }

    public DateTime? EndedAtUtc { get; init; }

    public required bool HasStarted { get; init; }

    public required bool HasEnded { get; init; }

    public required bool CanOpenClassroom { get; init; }

    public required bool HasExam { get; init; }

    public int? ExamStatus { get; init; }

    public required int ReviewCount { get; init; }

    public required decimal RatingAverage { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
