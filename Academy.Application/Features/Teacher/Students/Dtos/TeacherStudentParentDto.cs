namespace Academy.Application.Features.Teacher.Students.Dtos;

public sealed class TeacherStudentParentDto
{
    public required int ParentStudentId { get; init; }

    public required string FullName { get; init; }

    public string? PhoneNumber { get; init; }
}
