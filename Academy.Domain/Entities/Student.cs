using Academy.Domain.Common;

namespace Academy.Domain.Entities;

public class Student : BaseEntity
{
    public int UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public bool IsParent { get; set; }

    /// <summary>
    /// Unique code used by teachers and parents to link this student.
    /// Null for parent profiles.
    /// </summary>
    public string? StudentCode { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<LessonBooking> Bookings { get; set; } = [];

    public ICollection<LessonGroupMember> GroupMemberships { get; set; } = [];

    public ICollection<ExamAttempt> ExamAttempts { get; set; } = [];

    public ICollection<TeacherReview> Reviews { get; set; } = [];

    public ICollection<LessonReview> LessonReviews { get; set; } = [];

    public ICollection<SessionReview> SessionReviews { get; set; } = [];
}
