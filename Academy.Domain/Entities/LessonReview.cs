using Academy.Domain.Common;

namespace Academy.Domain.Entities;

public class LessonReview : BaseEntity
{
    public int LessonId { get; set; }

    public Lesson Lesson { get; set; } = null!;

    public int TeacherId { get; set; }

    public Teacher Teacher { get; set; } = null!;

    public int StudentId { get; set; }

    public Student Student { get; set; } = null!;

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }
}
