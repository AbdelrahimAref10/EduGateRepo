namespace Academy.Application.Features.Teacher.Lessons.Dtos;

public sealed class LessonGroupManageDto
{
    public required LessonGroupDto Group { get; init; }

    public required IReadOnlyList<LessonStudentDto> Students { get; init; }
}
