using Academy.Domain.Common;

namespace Academy.Domain.Entities;

/// <summary>
/// Links a Parent profile (Students.IsParent) to a real student child.
/// </summary>
public class ParentChildLink : BaseEntity
{
    public int ParentStudentId { get; set; }

    public Student ParentStudent { get; set; } = null!;

    public int ChildStudentId { get; set; }

    public Student ChildStudent { get; set; } = null!;

    public DateTime LinkedAtUtc { get; set; } = DateTime.UtcNow;
}
