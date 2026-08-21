using Academy.Domain.Common;
using Academy.Domain.Enums;

namespace Academy.Domain.Entities;

public class LessonBooking : BaseEntity
{
    public int LessonId { get; set; }

    public Lesson Lesson { get; set; } = null!;

    public int TeacherId { get; set; }

    public Teacher Teacher { get; set; } = null!;

    public int StudentId { get; set; }

    public Student Student { get; set; } = null!;

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAtUtc { get; set; }
}
