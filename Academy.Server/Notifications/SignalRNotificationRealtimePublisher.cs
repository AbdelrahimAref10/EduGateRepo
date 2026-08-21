using Academy.Application.Contracts.Notifications;
using Academy.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Academy.Server.Notifications;

public sealed class SignalRNotificationRealtimePublisher(IHubContext<NotificationsHub> hubContext)
    : INotificationRealtimePublisher
{
    public async Task PublishToUserAsync(
        int userId,
        NotificationPushDto notification,
        CancellationToken cancellationToken = default)
    {
        // Explicit camelCase payload so the Angular client always receives stable keys.
        var payload = new
        {
            id = notification.Id,
            notificationId = notification.NotificationId,
            titleAr = notification.TitleAr,
            titleEn = notification.TitleEn,
            bodyAr = notification.BodyAr,
            bodyEn = notification.BodyEn,
            isRead = notification.IsRead,
            type = notification.Type,
            entityType = notification.EntityType,
            entityId = notification.EntityId,
            userTargetId = notification.UserTargetId,
            createdAtUtc = notification.CreatedAtUtc
        };

        var userIdText = userId.ToString();
        var group = NotificationsHub.UserGroup(userId);

        // Prefer Identity-based Users mapping, and also target the manual group as fallback.
        await hubContext.Clients.User(userIdText)
            .SendAsync("notificationReceived", payload, cancellationToken);

        await hubContext.Clients.Group(group)
            .SendAsync("notificationReceived", payload, cancellationToken);
    }
}
