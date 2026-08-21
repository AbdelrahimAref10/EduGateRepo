namespace Academy.Application.Features.Student.Lessons.Dtos;

public sealed class BookingDto
{
    public required int Id { get; init; }

    public required int LessonId { get; init; }

    public required int TeacherId { get; init; }

    public required int StudentId { get; init; }

    public required string Status { get; init; }

    public required string Subject { get; init; }

    public required string EducationTypeName { get; init; }

    public required string EducationYearName { get; init; }

    public required DateOnly StartDate { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
