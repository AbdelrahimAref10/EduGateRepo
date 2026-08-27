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
            .AsNoTracking()
            .Where(s =>
                s.StartedAtUtc == null
                && s.EndedAtUtc == null
                && s.StartingSoonReminderSentAtUtc == null
                && s.SessionDate >= fromDate
                && s.SessionDate <= toDate)
            .Select(s => new
            {
                s.Id,
                s.LessonGroupId,
                s.SessionDate,
                s.StartTime,
                GroupName = s.LessonGroup.Name,
                Subject = s.LessonGroup.Lesson.Subject
            })
            .ToListAsync(cancellationToken);

        var sent = 0;
        var remindedIds = new List<int>();

        foreach (var session in candidates)
        {
            var startUtc = session.SessionDate.ToDateTime(session.StartTime, DateTimeKind.Utc);
            if (startUtc < now || startUtc > windowEnd)
                continue;

            var members = await dbContext.LessonGroupMembers
                .Where(m => m.LessonGroupId == session.LessonGroupId)
                .Select(m => new { m.StudentId, m.Student.UserId })
                .ToListAsync(cancellationToken);

            remindedIds.Add(session.Id);

            if (members.Count == 0)
                continue;

            var subject = session.Subject;
            var groupName = session.GroupName;
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

        if (remindedIds.Count > 0)
        {
            await dbContext.LessonGroupSessions
                .Where(s => remindedIds.Contains(s.Id))
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(s => s.StartingSoonReminderSentAtUtc, now),
                    cancellationToken);
        }

        if (sent > 0)
            logger.LogInformation("Sent starting-soon reminders for {Count} sessions.", sent);

        return sent;
    }
}
