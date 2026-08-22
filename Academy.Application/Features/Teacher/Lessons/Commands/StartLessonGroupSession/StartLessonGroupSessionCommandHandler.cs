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
        if (!session.StartedAtUtc.HasValue)
        {
            var now = DateTime.UtcNow;
            session.StartedAtUtc = now;

            if (!session.LessonGroup.StartedAtUtc.HasValue)
                session.LessonGroup.StartedAtUtc = now;

            if (!session.LessonGroup.Lesson.StartedAtUtc.HasValue)
            {
                session.LessonGroup.Lesson.StartedAtUtc = now;
                lessonJustStarted = true;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (lessonJustStarted)
        {
            var teacherName = teacher.User.FullName;
            var subject = session.LessonGroup.Lesson.Subject;
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
