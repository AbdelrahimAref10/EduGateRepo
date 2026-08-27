using Academy.Domain.Common;

namespace Academy.Domain.Entities;

/// <summary>
/// School calendar year managed by SuperAdmin (e.g. 2025/2026).
/// </summary>
public class AcademicYear : BaseEntity
{
    public string Name { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Lesson> Lessons { get; set; } = [];
}
