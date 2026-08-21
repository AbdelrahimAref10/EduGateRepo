using Academy.Domain.Common;
using Academy.Domain.Enums;

namespace Academy.Domain.Entities;

/// <summary>
/// Shared notification content. One notification can be delivered to many users via Details.
/// </summary>
public class Notification : BaseEntity
{
    public string TitleAr { get; set; } = null!;

    public string TitleEn { get; set; } = null!;

    public string BodyAr { get; set; } = null!;

    public string BodyEn { get; set; } = null!;

    public NotificationType Type { get; set; }

    public NotificationEntityType EntityType { get; set; }

    /// <summary>Related entity id (e.g. LessonId) used for navigation.</summary>
    public int? EntityId { get; set; }

    /// <summary>User the notification is about (e.g. student who booked).</summary>
    public int? UserTargetId { get; set; }

    public ApplicationUser? UserTarget { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<NotificationDetail> Details { get; set; } = [];
}
