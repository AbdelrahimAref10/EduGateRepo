using Academy.Application.Common.Images;
using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Notifications;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.StudentBookings.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.StudentBookings.Commands.ConfirmBooking;

public sealed class ConfirmBookingCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage,
    INotificationService notificationService)
    : IRequestHandler<ConfirmBookingCommand, Result<TeacherBookingDto>>
{
    public async Task<Result<TeacherBookingDto>> Handle(
        ConfirmBookingCommand request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<TeacherBookingDto>.NotFound("Teacher profile was not found.");

        var booking = await dbContext.LessonBookings
            .AsTracking()
            .Include(x => x.Lesson)
                .ThenInclude(x => x.EducationType)
            .Include(x => x.Lesson)
                .ThenInclude(x => x.EducationStage)
            .Include(x => x.Lesson)
                .ThenInclude(x => x.EducationYear)
            .Include(x => x.Student)
                .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(
                x => x.Id == request.BookingId && x.TeacherId == teacher.Id,
                cancellationToken);

        if (booking is null)
            return Result<TeacherBookingDto>.NotFound("Booking was not found.");

        if (booking.Status != BookingStatus.Pending)
            return Result<TeacherBookingDto>.Failure("Only pending bookings can be confirmed.");

        booking.Status = BookingStatus.Confirmed;
        booking.ReviewedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var teacherName = teacher.User.FullName;
        var subject = booking.Lesson.Subject;

        await notificationService.CreateAsync(
            new NotificationCreateRequest
            {
                RecipientUserIds = [booking.Student.UserId],
                UserTargetId = teacher.UserId,
                Type = NotificationType.LessonBookingConfirmed,
                EntityType = NotificationEntityType.Lesson,
                EntityId = booking.LessonId,
                TitleAr = "تم تأكيد الحجز",
                TitleEn = "Booking confirmed",
                BodyAr = $"المعلم {teacherName} أكد حجزك لدرس «{subject}». تاريخ البدء: {booking.Lesson.StartDate:yyyy-MM-dd}.",
                BodyEn = $"Teacher {teacherName} confirmed your booking for '{subject}'. Start date: {booking.Lesson.StartDate:yyyy-MM-dd}.",
                IncludeSuperAdmins = true
            },
            cancellationToken);

        var language = requestLanguage.Current;

        return Result<TeacherBookingDto>.Success(new TeacherBookingDto
        {
            Id = booking.Id,
            LessonId = booking.LessonId,
            TeacherId = booking.TeacherId,
            StudentId = booking.StudentId,
            StudentName = booking.Student.User.FullName,
            StudentPhotoUrl = ImageService.DisplayValue(booking.Student.User.ProfilePhoto),
            StudentCode = booking.Student.StudentCode,
            Subject = booking.Lesson.Subject,
            EducationTypeName = LocalizedNames.Pick(
                booking.Lesson.EducationType.NameAr,
                booking.Lesson.EducationType.NameEn,
                language),
            EducationStageName = LocalizedNames.Pick(
                booking.Lesson.EducationStage.NameAr,
                booking.Lesson.EducationStage.NameEn,
                language),
            EducationYearName = LocalizedNames.Pick(
                booking.Lesson.EducationYear.NameAr,
                booking.Lesson.EducationYear.NameEn,
                language),
            StartDate = booking.Lesson.StartDate,
            Status = booking.Status.ToString(),
            CreatedAtUtc = booking.CreatedAtUtc,
            ReviewedAtUtc = booking.ReviewedAtUtc
        });
    }
}
