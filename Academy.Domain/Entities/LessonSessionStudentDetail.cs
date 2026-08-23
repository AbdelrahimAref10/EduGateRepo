using Academy.Domain.Common;

namespace Academy.Domain.Entities;

/// <summary>
/// Per-student attendance and notes for a single session (classroom roster row).
/// Payment state lives in Charge / Payment tables — not here.
/// </summary>
public class LessonSessionStudentDetail : BaseEntity
{
    public int LessonGroupSessionId { get; set; }

    public LessonGroupSession LessonGroupSession { get; set; } = null!;

    public int StudentId { get; set; }

    public Student Student { get; set; } = null!;

    public bool IsPresent { get; set; }

    public string? TeacherNotes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }
}
