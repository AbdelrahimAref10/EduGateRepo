using Academy.Domain.Common;

namespace Academy.Domain.Entities;

/// <summary>
/// Study year within an education type (e.g. Year 1, Year 2, Year 3).
/// </summary>
public class EducationYear : BaseEntity
{
    public int EducationTypeId { get; set; }

    public EducationType EducationType { get; set; } = null!;

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Lesson> Lessons { get; set; } = [];
}
