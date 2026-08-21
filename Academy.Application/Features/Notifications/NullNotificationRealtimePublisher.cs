using Academy.Application.Contracts.Notifications;

namespace Academy.Application.Features.Notifications;

/// <summary>Fallback when SignalR publisher is not registered (e.g. design-time).</summary>
public sealed class NullNotificationRealtimePublisher : INotificationRealtimePublisher
{
    public Task PublishToUserAsync(
        int userId,
        NotificationPushDto notification,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
