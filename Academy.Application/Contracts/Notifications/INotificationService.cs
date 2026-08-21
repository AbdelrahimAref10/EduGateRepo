using Academy.Domain.Enums;

namespace Academy.Application.Contracts.Notifications;

public sealed class NotificationCreateRequest
{
    public required IReadOnlyList<int> RecipientUserIds { get; init; }

    public int? UserTargetId { get; init; }

    public required NotificationType Type { get; init; }

    public required NotificationEntityType EntityType { get; init; }

    public int? EntityId { get; init; }

    public required string TitleAr { get; init; }

    public required string TitleEn { get; init; }

    public required string BodyAr { get; init; }

    public required string BodyEn { get; init; }

    /// <summary>When true, all SuperAdmin users also receive the notification.</summary>
    public bool IncludeSuperAdmins { get; init; } = true;
}

public interface INotificationService
{
    Task CreateAsync(NotificationCreateRequest request, CancellationToken cancellationToken = default);
}
