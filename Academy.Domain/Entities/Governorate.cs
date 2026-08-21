using Academy.Domain.Common;

namespace Academy.Domain.Entities;

/// <summary>
/// Governorate / محافظة under a country.
/// </summary>
public class Governorate : BaseEntity
{
    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public int CountryId { get; set; }

    public Country Country { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public ICollection<City> Cities { get; set; } = [];
}
