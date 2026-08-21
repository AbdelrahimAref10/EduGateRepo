namespace Academy.Application.Features.Teacher.Classroom.Dtos;

public sealed class ClassroomStudentDetailDto
{
    public required int Id { get; init; }

    public required int StudentId { get; init; }

    public required string StudentName { get; init; }

    public string? StudentCode { get; init; }

    public required bool IsPresent { get; init; }

    public required bool IsPaid { get; init; }

    public string? TeacherNotes { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }
}
