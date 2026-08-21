using Academy.Domain.Common;

namespace Academy.Domain.Entities;

/// <summary>
/// Area / منطقة under a city.
/// </summary>
public class Area : BaseEntity
{
    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public int CityId { get; set; }

    public City City { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public ICollection<ApplicationUser> Users { get; set; } = [];

    public ICollection<Lesson> Lessons { get; set; } = [];

    public ICollection<LessonGroup> LessonGroups { get; set; } = [];
}
