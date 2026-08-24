using Academy.Application.Contracts.Notifications;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Parent;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Academy.Application.Features.System.Commands.SendSessionStartingSoonReminders;

public sealed record SendSessionStartingSoonRemindersCommand : IRequest<int>;

public sealed class SendSessionStartingSoonRemindersCommandHandler(
    IApplicationDbContext dbContext,
    INotificationService notificationService,
    ILogger<SendSessionStartingSoonRemindersCommandHandler> logger)
    : IRequestHandler<SendSessionStartingSoonRemindersCommand, int>
{
    private static readonly TimeSpan ReminderWindow = TimeSpan.FromMinutes(30);

    public async Task<int> Handle(
        SendSessionStartingSoonRemindersCommand request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var windowEnd = now.Add(ReminderWindow);
        var fromDate = DateOnly.FromDateTime(now);
        var toDate = DateOnly.FromDateTime(windowEnd);

        var candidates = await dbContext.LessonGroupSessions
            .AsTracking()
            .Include(s => s.LessonGroup)
                .ThenInclude(g => g.Lesson)
            .Where(s =>
                s.StartedAtUtc == null
                && s.EndedAtUtc == null
                && s.StartingSoonReminderSentAtUtc == null
                && s.SessionDate >= fromDate
                && s.SessionDate <= toDate)
            .ToListAsync(cancellationToken);

        var sent = 0;

        foreach (var session in candidates)
        {
            var startUtc = session.SessionDate.ToDateTime(session.StartTime, DateTimeKind.Utc);
            if (startUtc < now || startUtc > windowEnd)
                continue;

            var members = await dbContext.LessonGroupMembers
                .Where(m => m.LessonGroupId == session.LessonGroupId)
                .Select(m => new { m.StudentId, m.Student.UserId })
                .ToListAsync(cancellationToken);

            session.StartingSoonReminderSentAtUtc = now;

            if (members.Count == 0)
                continue;

            var subject = session.LessonGroup.Lesson.Subject;
            var groupName = session.LessonGroup.Name;
            var studentUserIds = members.Select(m => m.UserId).Distinct().ToList();

            await notificationService.CreateAsync(
                new NotificationCreateRequest
                {
                    RecipientUserIds = studentUserIds,
                    Type = NotificationType.SessionStartingSoon,
                    EntityType = NotificationEntityType.Session,
                    EntityId = session.Id,
                    TitleAr = "تذكير: الحصة قريبة",
                    TitleEn = "Reminder: session starting soon",
                    BodyAr = $"حصة مجموعة «{groupName}» في درس «{subject}» ستبدأ قريبًا.",
                    BodyEn = $"Session for group '{groupName}' in '{subject}' is starting soon.",
                    IncludeSuperAdmins = false
                },
                cancellationToken);

            await ParentNotifications.NotifyLinkedParentsForChildrenAsync(
                dbContext,
                notificationService,
                members.Select(m => m.StudentId),
                new NotificationCreateRequest
                {
                    RecipientUserIds = [],
                    Type = NotificationType.SessionStartingSoon,
                    EntityType = NotificationEntityType.Session,
                    EntityId = session.Id,
                    TitleAr = "تذكير: حصة ابنك/ابنتك قريبة",
                    TitleEn = "Reminder: your child's session soon",
                    BodyAr = $"حصة مجموعة «{groupName}» في درس «{subject}» ستبدأ قريبًا.",
                    BodyEn = $"Your child's session for group '{groupName}' in '{subject}' is starting soon.",
                    IncludeSuperAdmins = false
                },
                cancellationToken);

            sent++;
        }

        if (candidates.Count > 0)
            await dbContext.SaveChangesAsync(cancellationToken);

        if (sent > 0)
            logger.LogInformation("Sent starting-soon reminders for {Count} sessions.", sent);

        return sent;
    }
}
