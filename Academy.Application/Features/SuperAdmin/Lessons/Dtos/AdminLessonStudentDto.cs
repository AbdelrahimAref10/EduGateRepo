namespace Academy.Application.Features.SuperAdmin.Lessons.Dtos;

public sealed class AdminLessonStudentDto
{
    public required int BookingId { get; init; }

    public required int StudentId { get; init; }

    public required string StudentName { get; init; }

    public string? StudentCode { get; init; }

    public required string Status { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? ReviewedAtUtc { get; init; }

    public int? AssignedGroupId { get; init; }

    public string? AssignedGroupName { get; init; }
}
