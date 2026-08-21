using Academy.Domain.Common;

namespace Academy.Domain.Entities;

/// <summary>
/// Weekly schedule slot for a lesson group (day of week + time).
/// </summary>
public class LessonGroupDate : BaseEntity
{
    public int LessonGroupId { get; set; }

    public LessonGroup LessonGroup { get; set; } = null!;

    /// <summary>
    /// .NET DayOfWeek: Sunday=0 … Saturday=6.
    /// </summary>
    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }
}
