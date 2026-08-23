namespace Academy.Application.Features.Teacher.Classroom.Dtos;

public sealed class UpdateStudentSessionDetailRequest
{
    public bool IsPresent { get; set; }

    public string? TeacherNotes { get; set; }
}
