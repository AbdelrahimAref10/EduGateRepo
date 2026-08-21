namespace Academy.Application.Features.Teacher.Lessons.Dtos;

public sealed class LessonManageDto
{
    public required LessonDto Lesson { get; init; }

    public required IReadOnlyList<LessonStudentDto> Students { get; init; }

    public required IReadOnlyList<LessonGroupDto> Groups { get; init; }
}
