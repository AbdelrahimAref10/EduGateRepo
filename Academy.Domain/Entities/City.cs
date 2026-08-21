using Academy.Domain.Common;

namespace Academy.Domain.Entities;

/// <summary>
/// City / مدينة under a governorate.
/// </summary>
public class City : BaseEntity
{
    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public int GovernorateId { get; set; }

    public Governorate Governorate { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public ICollection<Area> Areas { get; set; } = [];
}
