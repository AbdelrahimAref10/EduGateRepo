namespace Academy.Application.Features.SuperAdmin.Lessons.Dtos;

public sealed class AdminLessonListItemDto
{
    public required int Id { get; init; }

    public required string Subject { get; init; }

    public required int TeacherId { get; init; }

    public required string TeacherName { get; init; }

    public required string AcademicYearName { get; init; }

    public required string EducationStageName { get; init; }

    public required string EducationYearName { get; init; }

    public required bool IsActive { get; init; }

    public DateTime? StartedAtUtc { get; init; }

    public required bool HasStarted { get; init; }

    public required int GroupsCount { get; init; }

    public required int BookingsCount { get; init; }

    public required int ConfirmedBookingsCount { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public required IReadOnlyList<AdminLessonStudentDto> Students { get; init; }
}
