using Academy.Domain.Common;

namespace Academy.Domain.Entities;

public class Country : BaseEntity
{
    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public string Code { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public ICollection<Governorate> Governorates { get; set; } = [];

    public ICollection<Lesson> Lessons { get; set; } = [];
}
