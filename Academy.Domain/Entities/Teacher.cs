using Academy.Domain.Common;

namespace Academy.Domain.Entities;

public class Teacher : BaseEntity
{
    public int UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Lesson> Lessons { get; set; } = [];

    public ICollection<LessonBooking> Bookings { get; set; } = [];
}
