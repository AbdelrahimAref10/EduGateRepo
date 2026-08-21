using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Notifications.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Notifications.Queries.GetMyNotifications;

public sealed class GetMyNotificationsQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetMyNotificationsQuery, Result<IReadOnlyList<NotificationDto>>>
{
    public async Task<Result<IReadOnlyList<NotificationDto>>> Handle(
        GetMyNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var language = requestLanguage.Current;
        var isArabic = language == AppLanguage.Arabic;

        var items = await dbContext.NotificationDetails
            .Where(x => x.UserId == request.UserId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(50)
            .Select(x => new NotificationDto
            {
                Id = x.Id,
                NotificationId = x.NotificationId,
                Title = isArabic ? x.Notification.TitleAr : x.Notification.TitleEn,
                Body = isArabic ? x.Notification.BodyAr : x.Notification.BodyEn,
                IsRead = x.IsRead,
                Type = x.Notification.Type.ToString(),
                EntityType = x.Notification.EntityType.ToString(),
                EntityId = x.Notification.EntityId,
                UserTargetId = x.Notification.UserTargetId,
                CreatedAtUtc = x.Notification.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<NotificationDto>>.Success(items);
    }
}
