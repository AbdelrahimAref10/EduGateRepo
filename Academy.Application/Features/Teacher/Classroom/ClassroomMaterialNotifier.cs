using Academy.Application.Contracts.Notifications;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Classroom;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Classroom;

internal static class ClassroomMaterialNotifier
{
    public static async Task NotifyGroupAsync(
        IApplicationDbContext dbContext,
        INotificationService notificationService,
        LessonGroupSession session,
        int teacherUserId,
        CancellationToken cancellationToken)
    {
        var studentUserIds = await dbContext.LessonGroupMembers
            .Where(x => x.LessonGroupId == session.LessonGroupId)
            .Select(x => x.Student.UserId)
            .ToListAsync(cancellationToken);

        if (studentUserIds.Count == 0)
            return;

        var teacherName = session.LessonGroup.Lesson.Teacher.User.FullName;
        var sessionNumber = await SessionNumbers.RankAsync(dbContext, session, cancellationToken);
        var label = string.IsNullOrWhiteSpace(session.Topic)
            ? null
            : session.Topic.Trim();
        var labelAr = label ?? $"الحصة {sessionNumber}";
        var labelEn = label ?? $"session {sessionNumber}";

        await notificationService.CreateAsync(
            new NotificationCreateRequest
            {
                RecipientUserIds = studentUserIds,
                UserTargetId = teacherUserId,
                Type = NotificationType.ClassroomMaterialAdded,
                EntityType = NotificationEntityType.Session,
                EntityId = session.Id,
                TitleAr = "مواد جديدة",
                TitleEn = "New materials",
                BodyAr = $"المعلم {teacherName} أضاف مواد جديدة لـ«{labelAr}».",
                BodyEn = $"Teacher {teacherName} added new materials to '{labelEn}'.",
                IncludeSuperAdmins = false
            },
            cancellationToken);
    }
}
