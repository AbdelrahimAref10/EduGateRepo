namespace Academy.Application.Features.Notifications.Dtos;

public sealed class NotificationDto
{
    /// <summary>NotificationDetail id (used for mark-as-read).</summary>
    public required int Id { get; init; }

    public required int NotificationId { get; init; }

    public required string Title { get; init; }

    public required string Body { get; init; }

    public required bool IsRead { get; init; }

    public required string Type { get; init; }

    public required string EntityType { get; init; }

    public int? EntityId { get; init; }

    public int? UserTargetId { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
