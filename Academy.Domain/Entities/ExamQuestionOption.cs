using Academy.Domain.Common;

namespace Academy.Domain.Entities;

public class ExamQuestionOption : BaseEntity
{
    public int ExamQuestionId { get; set; }

    public ExamQuestion Question { get; set; } = null!;

    public string Text { get; set; } = null!;

    public bool IsCorrect { get; set; }

    public int SortOrder { get; set; }
}
