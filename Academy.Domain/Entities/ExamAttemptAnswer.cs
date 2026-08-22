using Academy.Domain.Common;

namespace Academy.Domain.Entities;

public class ExamAttemptAnswer : BaseEntity
{
    public int ExamAttemptId { get; set; }

    public ExamAttempt Attempt { get; set; } = null!;

    public int ExamQuestionId { get; set; }

    public int? SelectedOptionId { get; set; }

    public bool IsCorrect { get; set; }
}
