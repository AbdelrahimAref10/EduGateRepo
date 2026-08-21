using Academy.Domain.Common;

namespace Academy.Domain.Entities;

/// <summary>
/// Per-recipient delivery row for a shared <see cref="Notification"/>.
/// </summary>
public class NotificationDetail : BaseEntity
{
    public int NotificationId { get; set; }

    public Notification Notification { get; set; } = null!;

    /// <summary>Recipient user id.</summary>
    public int UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
