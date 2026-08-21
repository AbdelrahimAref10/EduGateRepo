using Academy.Domain.Common;

namespace Academy.Domain.Entities;

/// <summary>
/// Curriculum subject belonging to a specific education year.
/// </summary>
public class EducationSubject : BaseEntity
{
    public int EducationYearId { get; set; }

    public EducationYear EducationYear { get; set; } = null!;

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Lesson> Lessons { get; set; } = [];
}
