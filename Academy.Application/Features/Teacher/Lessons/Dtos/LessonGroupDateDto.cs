namespace Academy.Application.Features.Teacher.Lessons.Dtos;

public sealed class LessonGroupDateDto
{
    public required int Id { get; init; }

    /// <summary>
    /// .NET DayOfWeek: Sunday=0 … Saturday=6.
    /// </summary>
    public required DayOfWeek DayOfWeek { get; init; }

    public required TimeOnly StartTime { get; init; }
}
