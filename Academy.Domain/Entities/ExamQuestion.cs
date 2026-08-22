using Academy.Domain.Common;

namespace Academy.Domain.Entities;

public class ExamQuestion : BaseEntity
{
    public int ExamId { get; set; }

    public Exam Exam { get; set; } = null!;

    public string Text { get; set; } = null!;

    public int SortOrder { get; set; }

    public ICollection<ExamQuestionOption> Options { get; set; } = [];
}
