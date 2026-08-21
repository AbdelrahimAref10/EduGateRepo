namespace Academy.Application.Features.Student.Classroom.Dtos;

public sealed class StudentClassroomSessionListItemDto
{
    public required int SessionId { get; init; }

    public required int LessonId { get; init; }

    public required int LessonGroupId { get; init; }

    public required string GroupName { get; init; }

    public required string Subject { get; init; }

    public required DateOnly SessionDate { get; init; }

    public required TimeOnly StartTime { get; init; }

    public string? Topic { get; init; }

    public required bool HasEnded { get; init; }

    public DateTime? StartedAtUtc { get; init; }

    public DateTime? EndedAtUtc { get; init; }

    public required string TeacherName { get; init; }

    public required bool CanOpenClassroom { get; init; }
}
