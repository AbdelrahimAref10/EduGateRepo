namespace Academy.Application.Features.Teacher.Students.Dtos;

public sealed class TeacherStudentListItemDto
{
    public required int StudentId { get; init; }

    public required string FullName { get; init; }

    public string? PhotoUrl { get; init; }

    public string? StudentCode { get; init; }

    public string? PhoneNumber { get; init; }

    public required int LessonsCount { get; init; }

    public required IReadOnlyList<TeacherStudentParentDto> Parents { get; init; }
}
