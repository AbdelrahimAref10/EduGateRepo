using Academy.Domain.Common;

namespace Academy.Domain.Entities;

/// <summary>
/// A single class occurrence generated from a group's weekly schedule within its date range.
/// </summary>
public class LessonGroupSession : BaseEntity
{
    public int LessonGroupId { get; set; }

    public LessonGroup LessonGroup { get; set; } = null!;

    public DateOnly SessionDate { get; set; }

    public TimeOnly StartTime { get; set; }

    /// <summary>Optional topic / lesson title for the classroom.</summary>
    public string? Topic { get; set; }

    /// <summary>Agenda / summary editable by the teacher in the classroom.</summary>
    public string? Description { get; set; }

    public DateTime? StartedAtUtc { get; set; }

    public DateTime? EndedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<LessonSessionStudentDetail> StudentDetails { get; set; } = [];

    public ICollection<LessonSessionMaterial> Materials { get; set; } = [];
}
