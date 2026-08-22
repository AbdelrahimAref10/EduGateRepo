namespace Academy.Application.Features.SuperAdmin.Lessons.Dtos;

public sealed class AdminGroupListItemDto
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required int LessonId { get; init; }

    public required string LessonSubject { get; init; }

    public required int TeacherId { get; init; }

    public required string TeacherName { get; init; }

    public required string AreaName { get; init; }

    public required string CityName { get; init; }

    public DateTime? StartedAtUtc { get; init; }

    public DateTime? EndedAtUtc { get; init; }

    public required bool HasStarted { get; init; }

    public required bool HasEnded { get; init; }

    public required int MembersCount { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public required IReadOnlyList<AdminGroupMemberDto> Members { get; init; }
}
