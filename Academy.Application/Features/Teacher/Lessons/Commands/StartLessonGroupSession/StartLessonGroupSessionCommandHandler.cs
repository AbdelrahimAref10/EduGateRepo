using Academy.Application.Common.Models;
using Academy.Application.Contracts.Notifications;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Classroom;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Commands.StartLessonGroupSession;

public sealed class StartLessonGroupSessionCommandHandler(
    IApplicationDbContext dbContext,
    INotificationService notificationService)
    : IRequestHandler<StartLessonGroupSessionCommand, Result<LessonGroupSessionDto>>
{
    public async Task<Result<LessonGroupSessionDto>> Handle(
        StartLessonGroupSessionCommand request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<LessonGroupSessionDto>.NotFound("Teacher profile was not found.");

        var session = await dbContext.LessonGroupSessions
            .AsTracking()
            .Include(x => x.LessonGroup)
                .ThenInclude(x => x.Lesson)
            .FirstOrDefaultAsync(
                x => x.Id == request.SessionId
                     && x.LessonGroupId == request.GroupId
                     && x.LessonGroup.LessonId == request.LessonId
                     && x.LessonGroup.Lesson.TeacherId == teacher.Id,
                cancellationToken);

        if (session is null)
            return Result<LessonGroupSessionDto>.NotFound("الحصة غير موجودة.");

        if (session.LessonGroup.EndedAtUtc.HasValue)
            return Result<LessonGroupSessionDto>.Conflict("المجموعة منتهية ولا يمكن بدء حصة جديدة.");

        if (session.EndedAtUtc.HasValue)
            return Result<LessonGroupSessionDto>.Conflict("هذه الحصة منتهية بالفعل.");

        var lessonJustStarted = false;
        var sessionJustStarted = false;
        if (!session.StartedAtUtc.HasValue)
        {
            var now = DateTime.UtcNow;
            session.StartedAtUtc = now;
            sessionJustStarted = true;

            if (!session.LessonGroup.StartedAtUtc.HasValue)
                session.LessonGroup.StartedAtUtc = now;

            if (!session.LessonGroup.Lesson.StartedAtUtc.HasValue)
            {
                session.LessonGroup.Lesson.StartedAtUtc = now;
                lessonJustStarted = true;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var teacherName = teacher.User.FullName;
        var subject = session.LessonGroup.Lesson.Subject;
        var groupName = session.LessonGroup.Name;

        if (sessionJustStarted)
        {
            var studentUserIds = await dbContext.LessonGroupMembers
                .Where(x => x.LessonGroupId == session.LessonGroupId)
                .Select(x => x.Student.UserId)
                .ToListAsync(cancellationToken);

            if (studentUserIds.Count > 0)
            {
                await notificationService.CreateAsync(
                    new NotificationCreateRequest
                    {
                        RecipientUserIds = studentUserIds,
                        UserTargetId = teacher.UserId,
                        Type = NotificationType.SessionStarted,
                        EntityType = NotificationEntityType.Session,
                        EntityId = session.Id,
                        TitleAr = "بدأت الحصة",
                        TitleEn = "Session started",
                        BodyAr = $"المعلم {teacherName} بدأ حصة مجموعة «{groupName}» في درس «{subject}».",
                        BodyEn = $"Teacher {teacherName} started a session for group '{groupName}' in '{subject}'.",
                        IncludeSuperAdmins = false
                    },
                    cancellationToken);
            }
        }

        if (lessonJustStarted)
        {
            await notificationService.CreateAsync(
                new NotificationCreateRequest
                {
                    RecipientUserIds = [],
                    UserTargetId = teacher.UserId,
                    Type = NotificationType.LessonStarted,
                    EntityType = NotificationEntityType.Lesson,
                    EntityId = session.LessonGroup.Lesson.Id,
                    TitleAr = "بدء درس",
                    TitleEn = "Lesson started",
                    BodyAr = $"بدأ المعلم {teacherName} درس «{subject}».",
                    BodyEn = $"Teacher {teacherName} started the lesson '{subject}'.",
                    IncludeSuperAdmins = true
                },
                cancellationToken);
        }

        await ClassroomSeeding.EnsureStudentDetailsAsync(dbContext, session, cancellationToken);

        return Result<LessonGroupSessionDto>.Success(
            LessonMappings.ToSessionDto(session, session.LessonGroup.EndedAtUtc.HasValue));
    }
}
