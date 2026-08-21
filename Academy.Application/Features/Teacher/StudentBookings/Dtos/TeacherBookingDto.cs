namespace Academy.Application.Features.Teacher.StudentBookings.Dtos;

public sealed class TeacherBookingDto
{
    public required int Id { get; init; }

    public required int LessonId { get; init; }

    public required int TeacherId { get; init; }

    public required int StudentId { get; init; }

    public required string StudentName { get; init; }

    public string? StudentCode { get; init; }

    public required string Subject { get; init; }

    public required string EducationTypeName { get; init; }

    public required string EducationStageName { get; init; }

    public required string EducationYearName { get; init; }

    public required DateOnly StartDate { get; init; }

    public required string Status { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? ReviewedAtUtc { get; init; }
}
