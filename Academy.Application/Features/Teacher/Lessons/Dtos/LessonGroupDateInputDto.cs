namespace Academy.Application.Features.Teacher.Lessons.Dtos;

public sealed class LessonGroupDateInputDto
{
    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }
}
