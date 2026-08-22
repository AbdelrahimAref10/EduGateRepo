namespace Academy.Application.Features.Teacher.Lessons.Dtos;

public sealed class LessonStudentDto
{
    public required int BookingId { get; init; }

    public required int StudentId { get; init; }

    public required string StudentName { get; init; }

    public string? PhotoUrl { get; init; }

    public string? StudentCode { get; init; }

    public required string Status { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? ReviewedAtUtc { get; init; }

    /// <summary>
    /// Group id if the student is already assigned to a group in this lesson.
    /// </summary>
    public int? AssignedGroupId { get; init; }

    public string? AssignedGroupName { get; init; }
}
