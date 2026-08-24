using Academy.Application.Contracts.Notifications;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Parent.Common;
using Academy.Domain.Enums;

namespace Academy.Application.Features.Parent;

internal static class ParentNotifications
{
    public static async Task NotifyLinkedParentsAsync(
        IApplicationDbContext dbContext,
        INotificationService notifications,
        int childStudentId,
        NotificationCreateRequest template,
        CancellationToken cancellationToken)
    {
        var parentUserIds = await ParentRecipientResolver.GetParentUserIdsForChildAsync(
            dbContext, childStudentId, cancellationToken);

        if (parentUserIds.Count == 0)
            return;

        await notifications.CreateAsync(
            new NotificationCreateRequest
            {
                RecipientUserIds = parentUserIds,
                UserTargetId = template.UserTargetId,
                Type = template.Type,
                EntityType = template.EntityType,
                EntityId = template.EntityId,
                TitleAr = template.TitleAr,
                TitleEn = template.TitleEn,
                BodyAr = template.BodyAr,
                BodyEn = template.BodyEn,
                IncludeSuperAdmins = false
            },
            cancellationToken);
    }

    public static async Task NotifyLinkedParentsForChildrenAsync(
        IApplicationDbContext dbContext,
        INotificationService notifications,
        IEnumerable<int> childStudentIds,
        NotificationCreateRequest template,
        CancellationToken cancellationToken)
    {
        var parentUserIds = await ParentRecipientResolver.GetParentUserIdsForChildrenAsync(
            dbContext, childStudentIds, cancellationToken);

        if (parentUserIds.Count == 0)
            return;

        await notifications.CreateAsync(
            new NotificationCreateRequest
            {
                RecipientUserIds = parentUserIds,
                UserTargetId = template.UserTargetId,
                Type = template.Type,
                EntityType = template.EntityType,
                EntityId = template.EntityId,
                TitleAr = template.TitleAr,
                TitleEn = template.TitleEn,
                BodyAr = template.BodyAr,
                BodyEn = template.BodyEn,
                IncludeSuperAdmins = false
            },
            cancellationToken);
    }

    public static Task NotifyAbsentAsync(
        IApplicationDbContext dbContext,
        INotificationService notifications,
        int childStudentId,
        string childName,
        string subject,
        int sessionId,
        CancellationToken cancellationToken) =>
        NotifyLinkedParentsAsync(
            dbContext,
            notifications,
            childStudentId,
            new NotificationCreateRequest
            {
                RecipientUserIds = [],
                Type = NotificationType.StudentAbsent,
                EntityType = NotificationEntityType.Session,
                EntityId = sessionId,
                TitleAr = "غياب الطالب",
                TitleEn = "Student absent",
                BodyAr = $"تم تسجيل غياب {childName} عن حصة «{subject}».",
                BodyEn = $"{childName} was marked absent for '{subject}'.",
                IncludeSuperAdmins = false
            },
            cancellationToken);

    public static Task NotifyPresentAsync(
        IApplicationDbContext dbContext,
        INotificationService notifications,
        int childStudentId,
        string childName,
        string subject,
        int sessionId,
        CancellationToken cancellationToken) =>
        NotifyLinkedParentsAsync(
            dbContext,
            notifications,
            childStudentId,
            new NotificationCreateRequest
            {
                RecipientUserIds = [],
                Type = NotificationType.StudentPresent,
                EntityType = NotificationEntityType.Session,
                EntityId = sessionId,
                TitleAr = "حضور الطالب",
                TitleEn = "Student present",
                BodyAr = $"تم تسجيل حضور {childName} في حصة «{subject}».",
                BodyEn = $"{childName} was marked present for '{subject}'.",
                IncludeSuperAdmins = false
            },
            cancellationToken);
}
