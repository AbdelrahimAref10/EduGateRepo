using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Notifications;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentEntity = Academy.Domain.Entities.Student;

namespace Academy.Application.Features.Teacher.Lessons.Commands.AddGroupMemberByCode;

public sealed class AddGroupMemberByCodeCommandHandler(
    IApplicationDbContext dbContext,
    INotificationService notificationService,
    IRequestLanguage requestLanguage)
    : IRequestHandler<AddGroupMemberByCodeCommand, Result<LessonGroupDto>>
{
    public async Task<Result<LessonGroupDto>> Handle(
        AddGroupMemberByCodeCommand request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<LessonGroupDto>.NotFound("Teacher profile was not found.");

        var group = await dbContext.LessonGroups
            .AsTracking()
            .Include(x => x.Lesson)
            .Include(x => x.Area)
                .ThenInclude(x => x.City)
            .Include(x => x.Members)
                .ThenInclude(x => x.Student)
                    .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(
                x => x.Id == request.GroupId
                     && x.LessonId == request.LessonId
                     && x.Lesson.TeacherId == teacher.Id,
                cancellationToken);

        if (group is null)
            return Result<LessonGroupDto>.NotFound("Group was not found.");

        if (group.EndedAtUtc.HasValue)
            return Result<LessonGroupDto>.Conflict("لا يمكن إضافة طلاب بعد إنهاء المجموعة.");

        if (group.MaxCapacity.HasValue && group.Members.Count >= group.MaxCapacity.Value)
            return Result<LessonGroupDto>.Failure("المجموعة ممتلئة.");

        var studentResult = await ResolveStudentAsync(request, cancellationToken);
        if (!studentResult.IsSuccess)
            return Result<LessonGroupDto>.Failure(studentResult.Error, studentResult.StatusCode);

        var student = studentResult.Value!;

        var booking = await dbContext.LessonBookings
            .FirstOrDefaultAsync(
                x => x.LessonId == request.LessonId && x.StudentId == student.Id,
                cancellationToken);

        if (booking is null)
            return Result<LessonGroupDto>.Failure("هذا الطالب غير حاجز لهذا الدرس.");

        if (booking.Status != BookingStatus.Confirmed)
            return Result<LessonGroupDto>.Failure("يجب تأكيد حجز الطالب قبل إضافته لمجموعة.");

        var alreadyInLessonGroup = await dbContext.LessonGroupMembers
            .AnyAsync(
                x => x.StudentId == student.Id && x.LessonGroup.LessonId == request.LessonId,
                cancellationToken);

        if (alreadyInLessonGroup)
            return Result<LessonGroupDto>.Conflict("الطالب موجود بالفعل في مجموعة لهذا الدرس.");

        group.Members.Add(new LessonGroupMember
        {
            StudentId = student.Id,
            AddedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.CreateAsync(
            new NotificationCreateRequest
            {
                RecipientUserIds = [student.UserId],
                UserTargetId = teacher.UserId,
                Type = NotificationType.StudentAddedToGroup,
                EntityType = NotificationEntityType.Lesson,
                EntityId = group.LessonId,
                TitleAr = "تمت إضافتك لمجموعة",
                TitleEn = "Added to a group",
                BodyAr = $"المعلم {teacher.User.FullName} أضافك لمجموعة «{group.Name}» في درس «{group.Lesson.Subject}».",
                BodyEn = $"Teacher {teacher.User.FullName} added you to group '{group.Name}' in '{group.Lesson.Subject}'.",
                IncludeSuperAdmins = true
            },
            cancellationToken);

        var refreshed = await dbContext.LessonGroups
            .Include(x => x.Area)
                .ThenInclude(x => x.City)
            .Include(x => x.Dates)
            .Include(x => x.Members)
                .ThenInclude(x => x.Student)
                    .ThenInclude(x => x.User)
            .FirstAsync(x => x.Id == group.Id, cancellationToken);

        return Result<LessonGroupDto>.Success(
            LessonMappings.ToGroupDto(refreshed, requestLanguage.Current));
    }

    private async Task<Result<StudentEntity>> ResolveStudentAsync(
        AddGroupMemberByCodeCommand request,
        CancellationToken cancellationToken)
    {
        if (request.StudentId is > 0)
        {
            var byId = await dbContext.Students
                .Include(x => x.User)
                .FirstOrDefaultAsync(
                    x => x.Id == request.StudentId.Value && !x.IsParent,
                    cancellationToken);

            return byId is null
                ? Result<StudentEntity>.NotFound("لم يتم العثور على الطالب.")
                : Result<StudentEntity>.Success(byId);
        }

        var code = request.StudentCode!.Trim();

        var student = await dbContext.Students
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.StudentCode != null
                     && x.StudentCode == code
                     && !x.IsParent,
                cancellationToken);

        if (student is null)
        {
            var codeUpper = code.ToUpperInvariant();
            student = await dbContext.Students
                .Include(x => x.User)
                .Where(x => x.StudentCode != null && !x.IsParent)
                .FirstOrDefaultAsync(
                    x => x.StudentCode!.ToUpper() == codeUpper,
                    cancellationToken);
        }

        return student is null
            ? Result<StudentEntity>.NotFound("لم يتم العثور على طالب بهذا الكود.")
            : Result<StudentEntity>.Success(student);
    }
}
