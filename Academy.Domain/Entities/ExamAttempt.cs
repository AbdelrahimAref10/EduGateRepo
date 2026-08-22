using Academy.Domain.Common;

namespace Academy.Domain.Entities;

public class ExamAttempt : BaseEntity
{
    public int ExamId { get; set; }

    public Exam Exam { get; set; } = null!;

    public int StudentId { get; set; }

    public Student Student { get; set; } = null!;

    public int Score { get; set; }

    public int MaxScore { get; set; }

    public int CurrentQuestionIndex { get; set; }

    public DateTime CurrentQuestionStartedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? SubmittedAtUtc { get; set; }

    public ICollection<ExamAttemptAnswer> Answers { get; set; } = [];
}
