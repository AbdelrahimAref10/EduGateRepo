namespace Academy.Application.Features.Student.Classroom.Dtos;

public sealed class StudentClassroomClassmateDto
{
    public required string StudentName { get; init; }

    public string? PhotoUrl { get; init; }

    public string? StudentCode { get; init; }
}
