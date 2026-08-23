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

    public decimal RatingAverage { get; set; }

    public int RatingCount { get; set; }

    /// <summary>True when this session is a makeup class for selected students only.</summary>
    public bool IsMakeup { get; set; }

    public int? MakeupForSessionId { get; set; }

    public LessonGroupSession? MakeupForSession { get; set; }

    public ICollection<LessonGroupSession> MakeupSessions { get; set; } = [];

    public ICollection<LessonSessionStudentDetail> StudentDetails { get; set; } = [];

    public ICollection<LessonSessionMaterial> Materials { get; set; } = [];

    public ICollection<SessionReview> Reviews { get; set; } = [];

    public Exam? Exam { get; set; }

    public static LessonGroupSession CreateMakeup(
        int lessonGroupId,
        DateOnly sessionDate,
        TimeOnly startTime,
        string? topic,
        int? makeupForSessionId)
    {
        return new LessonGroupSession
        {
            LessonGroupId = lessonGroupId,
            SessionDate = sessionDate,
            StartTime = startTime,
            Topic = string.IsNullOrWhiteSpace(topic) ? "حصة تعويض" : topic.Trim(),
            IsMakeup = true,
            MakeupForSessionId = makeupForSessionId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
