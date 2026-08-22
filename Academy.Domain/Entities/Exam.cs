using Academy.Domain.Common;
using Academy.Domain.Enums;

namespace Academy.Domain.Entities;

/// <summary>
/// One AI-generated exam per classroom session.
/// </summary>
public class Exam : BaseEntity
{
    public int LessonGroupSessionId { get; set; }

    public LessonGroupSession LessonGroupSession { get; set; } = null!;

    public string Title { get; set; } = null!;

    public ExamStatus Status { get; set; } = ExamStatus.Draft;

    public int CreatedByUserId { get; set; }

    public ApplicationUser CreatedByUser { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? PublishedAtUtc { get; set; }

    /// <summary>Time allowed for each question, set by the teacher in minutes then stored as seconds.</summary>
    public int SecondsPerQuestion { get; set; } = 600;

    public ICollection<ExamQuestion> Questions { get; set; } = [];

    public ICollection<ExamAttempt> Attempts { get; set; } = [];
}
