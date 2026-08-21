using Academy.Domain.Common;

namespace Academy.Domain.Entities;

public class SuperAdmin : BaseEntity
{
    public int UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
