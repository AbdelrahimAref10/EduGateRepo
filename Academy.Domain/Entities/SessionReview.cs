using Academy.Domain.Common;

namespace Academy.Domain.Entities;

public class SessionReview : BaseEntity
{
    public int LessonGroupSessionId { get; set; }

    public LessonGroupSession LessonGroupSession { get; set; } = null!;

    public int TeacherId { get; set; }

    public Teacher Teacher { get; set; } = null!;

    public int StudentId { get; set; }

    public Student Student { get; set; } = null!;

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }
}
