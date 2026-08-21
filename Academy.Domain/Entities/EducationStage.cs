using Academy.Domain.Common;

namespace Academy.Domain.Entities;

/// <summary>
/// Educational stage under an education type (e.g. Primary, Preparatory).
/// </summary>
public class EducationStage : BaseEntity
{
    public int EducationTypeId { get; set; }

    public EducationType EducationType { get; set; } = null!;

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<EducationYear> Years { get; set; } = [];

    public ICollection<Lesson> Lessons { get; set; } = [];
}
