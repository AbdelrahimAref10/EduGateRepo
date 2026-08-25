using Academy.Application.Common.Models;
using Academy.Application.Contracts.Notifications;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Parent;
using Academy.Application.Features.Teacher.Students.Common;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Students.Commands.TransferStudentGroup;

public sealed class TransferStudentGroupCommandHandler(
    IApplicationDbContext dbContext,
    INotificationService notificationService)
    : IRequestHandler<TransferStudentGroupCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        TransferStudentGroupCommand request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<int>.NotFound("Teacher profile was not found.");

        var isStudent = await TeacherStudentAccess.IsTeachersConfirmedStudentAsync(
            dbContext, teacher.Id, request.StudentId, cancellationToken);

        if (!isStudent)
            return Result<int>.NotFound("Student was not found.");

        var membership = await dbContext.LessonGroupMembers
            .Include(x => x.LessonGroup)
                .ThenInclude(x => x.Lesson)
            .Include(x => x.Student)
                .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(
                x => x.StudentId == request.StudentId
                     && x.LessonGroup.LessonId == request.LessonId
                     && x.LessonGroup.Lesson.TeacherId == teacher.Id,
                cancellationToken);

        if (membership is null)
            return Result<int>.NotFound("الطالب غير مشترك في مجموعة لهذا الدرس.");

        var targetGroup = await dbContext.LessonGroups
            .Include(x => x.Members)
            .FirstOrDefaultAsync(
                x => x.Id == request.TargetGroupId
                     && x.LessonId == request.LessonId
                     && x.Lesson.TeacherId == teacher.Id,
                cancellationToken);

        if (targetGroup is null)
            return Result<int>.NotFound("Group was not found.");

        var previousGroupName = membership.LessonGroup.Name;
        var lessonSubject = membership.LessonGroup.Lesson.Subject;
        var studentName = membership.Student.User.FullName;
        var studentUserId = membership.Student.UserId;
        var error = membership.TryMoveTo(targetGroup);

        if (error is not null)
        {
            if (targetGroup.Id == membership.LessonGroupId || targetGroup.HasEnded)
                return Result<int>.Conflict(error);

            return Result<int>.Failure(error);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.CreateAsync(
            new NotificationCreateRequest
            {
                RecipientUserIds = [studentUserId],
                UserTargetId = teacher.UserId,
                Type = NotificationType.StudentAddedToGroup,
                EntityType = NotificationEntityType.Lesson,
                EntityId = request.LessonId,
                TitleAr = "تم نقلك لمجموعة أخرى",
                TitleEn = "Moved to another group",
                BodyAr = $"المعلم {teacher.User.FullName} نقلك من مجموعة «{previousGroupName}» إلى مجموعة «{targetGroup.Name}» في درس «{lessonSubject}».",
                BodyEn = $"Teacher {teacher.User.FullName} moved you from group '{previousGroupName}' to '{targetGroup.Name}' in '{lessonSubject}'.",
                IncludeSuperAdmins = true
            },
            cancellationToken);

        await ParentNotifications.NotifyLinkedParentsAsync(
            dbContext,
            notificationService,
            request.StudentId,
            new NotificationCreateRequest
            {
                RecipientUserIds = [],
                UserTargetId = teacher.UserId,
                Type = NotificationType.StudentAddedToGroup,
                EntityType = NotificationEntityType.Lesson,
                EntityId = request.LessonId,
                TitleAr = "تم نقل الطالب لمجموعة أخرى",
                TitleEn = "Student moved to another group",
                BodyAr = $"تم نقل {studentName} من مجموعة «{previousGroupName}» إلى مجموعة «{targetGroup.Name}» في درس «{lessonSubject}».",
                BodyEn = $"{studentName} was moved from group '{previousGroupName}' to '{targetGroup.Name}' in '{lessonSubject}'.",
                IncludeSuperAdmins = false
            },
            cancellationToken);

        return Result<int>.Success(targetGroup.Id);
    }
}
