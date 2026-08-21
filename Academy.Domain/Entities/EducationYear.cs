using Academy.Domain.Common;

namespace Academy.Domain.Entities;

/// <summary>
/// Study year within an education stage (e.g. Year 1, Year 2).
/// </summary>
public class EducationYear : BaseEntity
{
    public int EducationStageId { get; set; }

    public EducationStage EducationStage { get; set; } = null!;

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<EducationSubject> Subjects { get; set; } = [];

    public ICollection<Lesson> Lessons { get; set; } = [];
}
