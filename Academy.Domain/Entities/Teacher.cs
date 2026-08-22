using Academy.Domain.Common;

namespace Academy.Domain.Entities;

public class Teacher : BaseEntity
{
    public int UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public decimal RatingAverage { get; set; }

    public int RatingCount { get; set; }

    public ICollection<Lesson> Lessons { get; set; } = [];

    public ICollection<LessonBooking> Bookings { get; set; } = [];

    public ICollection<TeacherReview> Reviews { get; set; } = [];

    public ICollection<LessonReview> LessonReviews { get; set; } = [];

    public ICollection<SessionReview> SessionReviews { get; set; } = [];
}
