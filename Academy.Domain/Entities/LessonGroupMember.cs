using Academy.Domain.Common;

namespace Academy.Domain.Entities;

/// <summary>
/// Student assignment to a lesson group (typically added by student code).
/// </summary>
public class LessonGroupMember : BaseEntity
{
    public int LessonGroupId { get; set; }

    public LessonGroup LessonGroup { get; set; } = null!;

    public int StudentId { get; set; }

    public Student Student { get; set; } = null!;

    public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;
}
