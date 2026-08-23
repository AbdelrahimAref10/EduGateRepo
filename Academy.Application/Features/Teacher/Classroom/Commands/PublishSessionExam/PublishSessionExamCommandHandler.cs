using Academy.Application.Common.Models;
using Academy.Application.Contracts.Notifications;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Classroom.Exams;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Classroom.Commands.PublishSessionExam;

public sealed class PublishSessionExamCommandHandler(
    IApplicationDbContext dbContext,
    INotificationService notificationService)
    : IRequestHandler<PublishSessionExamCommand, Result<TeacherExamDto>>
{
    public async Task<Result<TeacherExamDto>> Handle(
        PublishSessionExamCommand request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<TeacherExamDto>.NotFound("Teacher profile was not found.");

        var session = await TeacherClassroomLoader.LoadOwnedSessionAsync(
            dbContext,
            teacher.Id,
            request.SessionId,
            cancellationToken);

        if (session is null)
            return Result<TeacherExamDto>.NotFound("الحصة غير موجودة.");

        var exam = await dbContext.Exams
            .AsTracking()
            .Include(x => x.Questions)
                .ThenInclude(x => x.Options)
            .FirstOrDefaultAsync(x => x.LessonGroupSessionId == request.SessionId, cancellationToken);

        if (exam is null)
            return Result<TeacherExamDto>.NotFound("لا يوجد امتحان لهذه الحصة.");

        if (exam.Questions.Count == 0)
            return Result<TeacherExamDto>.Failure("الامتحان لا يحتوي على أسئلة.");

        if (exam.Status != ExamStatus.Published)
        {
            exam.Status = ExamStatus.Published;
            exam.PublishedAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);

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
                        UserTargetId = request.UserId,
                        Type = NotificationType.ExamPublished,
                        EntityType = NotificationEntityType.Session,
                        EntityId = request.SessionId,
                        TitleAr = "امتحان جديد",
                        TitleEn = "New exam",
                        BodyAr = $"المعلم {teacher.User.FullName} أنشأ امتحان «{exam.Title}». ادخل وحله.",
                        BodyEn = $"Teacher {teacher.User.FullName} created the exam '{exam.Title}'. Open it and start.",
                        IncludeSuperAdmins = false
                    },
                    cancellationToken);
            }
        }

        return Result<TeacherExamDto>.Success(ExamMappings.ToTeacherDto(exam));
    }
}
