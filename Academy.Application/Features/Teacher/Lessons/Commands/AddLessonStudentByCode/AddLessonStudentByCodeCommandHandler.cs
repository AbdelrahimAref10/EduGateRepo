using Academy.Application.Common.Images;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Notifications;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Commands.AddLessonStudentByCode;

public sealed class AddLessonStudentByCodeCommandHandler(
    IApplicationDbContext dbContext,
    INotificationService notificationService)
    : IRequestHandler<AddLessonStudentByCodeCommand, Result<LessonStudentDto>>
{
    public async Task<Result<LessonStudentDto>> Handle(
        AddLessonStudentByCodeCommand request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<LessonStudentDto>.NotFound("Teacher profile was not found.");

        var lesson = await dbContext.Lessons
            .FirstOrDefaultAsync(
                x => x.Id == request.LessonId && x.TeacherId == teacher.Id,
                cancellationToken);

        if (lesson is null)
            return Result<LessonStudentDto>.NotFound("Lesson was not found.");

        var code = request.StudentCode.Trim();

        var student = await dbContext.Students
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.StudentCode != null && x.StudentCode == code && !x.IsParent,
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

        if (student is null)
            return Result<LessonStudentDto>.NotFound("لم يتم العثور على طالب بهذا الكود.");

        var booking = await dbContext.LessonBookings
            .AsTracking()
            .FirstOrDefaultAsync(
                x => x.LessonId == lesson.Id && x.StudentId == student.Id,
                cancellationToken);

        NotificationType notifyType;
        string titleAr;
        string titleEn;
        string bodyAr;
        string bodyEn;

        if (booking is null)
        {
            booking = new LessonBooking
            {
                LessonId = lesson.Id,
                TeacherId = teacher.Id,
                StudentId = student.Id,
                Status = BookingStatus.Confirmed,
                CreatedAtUtc = DateTime.UtcNow,
                ReviewedAtUtc = DateTime.UtcNow
            };

            dbContext.LessonBookings.Add(booking);

            notifyType = NotificationType.StudentAddedToLesson;
            titleAr = "تمت إضافتك لدرس";
            titleEn = "Added to lesson";
            bodyAr = $"المعلم {teacher.User.FullName} أضافك لدرس «{lesson.Subject}».";
            bodyEn = $"Teacher {teacher.User.FullName} added you to '{lesson.Subject}'.";
        }
        else if (booking.Status == BookingStatus.Confirmed)
        {
            return Result<LessonStudentDto>.Conflict("الطالب مسجل بالفعل في هذا الدرس.");
        }
        else
        {
            booking.Status = BookingStatus.Confirmed;
            booking.ReviewedAtUtc = DateTime.UtcNow;

            notifyType = NotificationType.LessonBookingConfirmed;
            titleAr = "تم تأكيد الحجز";
            titleEn = "Booking confirmed";
            bodyAr = $"المعلم {teacher.User.FullName} أكد حجزك لدرس «{lesson.Subject}».";
            bodyEn = $"Teacher {teacher.User.FullName} confirmed your booking for '{lesson.Subject}'.";
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.CreateAsync(
            new NotificationCreateRequest
            {
                RecipientUserIds = [student.UserId],
                UserTargetId = teacher.UserId,
                Type = notifyType,
                EntityType = NotificationEntityType.Lesson,
                EntityId = lesson.Id,
                TitleAr = titleAr,
                TitleEn = titleEn,
                BodyAr = bodyAr,
                BodyEn = bodyEn,
                IncludeSuperAdmins = true
            },
            cancellationToken);

        var assignedGroup = await dbContext.LessonGroupMembers
            .Where(x => x.StudentId == student.Id && x.LessonGroup.LessonId == lesson.Id)
            .Select(x => new { x.LessonGroupId, x.LessonGroup.Name })
            .FirstOrDefaultAsync(cancellationToken);

        return Result<LessonStudentDto>.Success(new LessonStudentDto
        {
            BookingId = booking.Id,
            StudentId = student.Id,
            StudentName = student.User.FullName,
            PhotoUrl = ImageService.DisplayValue(student.User.ProfilePhoto),
            StudentCode = student.StudentCode,
            Status = booking.Status.ToString(),
            CreatedAtUtc = booking.CreatedAtUtc,
            ReviewedAtUtc = booking.ReviewedAtUtc,
            AssignedGroupId = assignedGroup?.LessonGroupId,
            AssignedGroupName = assignedGroup?.Name
        });
    }
}
