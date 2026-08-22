namespace Academy.Application.Features.Teacher.Lessons.Dtos;

public sealed class LessonGroupMemberDto
{
    public required int Id { get; init; }

    public required int StudentId { get; init; }

    public required string StudentName { get; init; }

    public string? PhotoUrl { get; init; }

    public string? StudentCode { get; init; }

    public required DateTime AddedAtUtc { get; init; }
}
