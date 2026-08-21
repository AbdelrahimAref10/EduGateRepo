using Academy.Application.Features.Teacher.Classroom.Dtos;

namespace Academy.Application.Features.Student.Classroom.Dtos;

public sealed class StudentClassroomDto
{
    public required int SessionId { get; init; }

    public required int LessonId { get; init; }

    public required int LessonGroupId { get; init; }

    public required string GroupName { get; init; }

    public required string Subject { get; init; }

    public required DateOnly SessionDate { get; init; }

    public required TimeOnly StartTime { get; init; }

    public string? Topic { get; init; }

    public string? Description { get; init; }

    public required bool HasStarted { get; init; }

    public required bool HasEnded { get; init; }

    public DateTime? StartedAtUtc { get; init; }

    public DateTime? EndedAtUtc { get; init; }

    public required string TeacherName { get; init; }

    public ClassroomStudentDetailDto? MyDetail { get; init; }

    public IReadOnlyList<StudentClassroomClassmateDto> Classmates { get; init; } = [];

    public IReadOnlyList<ClassroomMaterialDto> Materials { get; init; } = [];
}
