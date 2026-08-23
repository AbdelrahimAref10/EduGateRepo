using Academy.Application.Contracts.Notifications;
using Academy.Application.Contracts.Persistence;
using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Notifications;

public sealed class NotificationService(
    IApplicationDbContext dbContext,
    INotificationRealtimePublisher realtimePublisher) : INotificationService
{
    public async Task CreateAsync(
        NotificationCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var recipientIds = request.RecipientUserIds
            .Where(id => id > 0)
            .Distinct()
            .ToHashSet();

        if (request.IncludeSuperAdmins)
        {
            var adminIds = await dbContext.SuperAdmins
                .Select(x => x.UserId)
                .ToListAsync(cancellationToken);

            foreach (var id in adminIds)
                recipientIds.Add(id);
        }

        if (recipientIds.Count == 0)
            return;

        var now = DateTime.UtcNow;

        var notification = new Notification
        {
            TitleAr = request.TitleAr.Trim(),
            TitleEn = request.TitleEn.Trim(),
            BodyAr = request.BodyAr.Trim(),
            BodyEn = request.BodyEn.Trim(),
            Type = request.Type,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            UserTargetId = request.UserTargetId,
            CreatedAtUtc = now,
            Details = recipientIds.Select(userId => new NotificationDetail
            {
                UserId = userId,
                IsRead = false,
                CreatedAtUtc = now
            }).ToList()
        };

        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var detail in notification.Details)
        {
            var payload = new NotificationPushDto
            {
                Id = detail.Id,
                NotificationId = notification.Id,
                TitleAr = notification.TitleAr,
                TitleEn = notification.TitleEn,
                BodyAr = notification.BodyAr,
                BodyEn = notification.BodyEn,
                IsRead = false,
                Type = notification.Type.ToString(),
                EntityType = notification.EntityType.ToString(),
                EntityId = notification.EntityId,
                UserTargetId = notification.UserTargetId,
                CreatedAtUtc = notification.CreatedAtUtc
            };

            await realtimePublisher.PublishToUserAsync(detail.UserId, payload, cancellationToken);
        }
    }
}

