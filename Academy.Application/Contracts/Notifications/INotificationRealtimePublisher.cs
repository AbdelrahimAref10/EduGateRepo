namespace Academy.Application.Contracts.Notifications;

public sealed class NotificationPushDto
{
    /// <summary>NotificationDetail id (mark-as-read key).</summary>
    public required int Id { get; init; }

    public required int NotificationId { get; init; }

    public required string TitleAr { get; init; }

    public required string TitleEn { get; init; }

    public required string BodyAr { get; init; }

    public required string BodyEn { get; init; }

    public required bool IsRead { get; init; }

    public required string Type { get; init; }

    public required string EntityType { get; init; }

    public int? EntityId { get; init; }

    public int? UserTargetId { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}

public interface INotificationRealtimePublisher
{
    Task PublishToUserAsync(int userId, NotificationPushDto notification, CancellationToken cancellationToken = default);
}
