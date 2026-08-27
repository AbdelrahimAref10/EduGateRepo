using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Notifications;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Marketplace;
using Academy.Application.Features.Student.Lessons.Dtos;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Student.Lessons.Commands.BookLesson;

public sealed class BookLessonCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage,
    INotificationService notificationService)
    : IRequestHandler<BookLessonCommand, Result<BookingDto>>
{
    public async Task<Result<BookingDto>> Handle(
        BookLessonCommand request,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && !x.IsParent, cancellationToken);

        if (student is null)
            return Result<BookingDto>.NotFound("Student profile was not found.");

        var lesson = await dbContext.Lessons
            .Include(x => x.AcademicYear)
            .Include(x => x.EducationStage)
            .Include(x => x.EducationYear)
            .Include(x => x.Teacher)
            .FirstOrDefaultAsync(x => x.Id == request.LessonId && x.IsActive, cancellationToken);

        if (lesson is null)
            return Result<BookingDto>.NotFound("Lesson was not found.");

        var alreadyBooked = await dbContext.LessonBookings
            .AnyAsync(x => x.LessonId == lesson.Id && x.StudentId == student.Id, cancellationToken);

        if (alreadyBooked)
            return Result<BookingDto>.Conflict("You already booked this lesson.");

        var seats = await LessonSeatLookup.ForLessonsAsync(dbContext, [lesson.Id], cancellationToken);
        if (seats.GetValueOrDefault(lesson.Id, LessonSeatAvailability.Open()).IsFull)
            return Result<BookingDto>.Conflict("This lesson has no remaining seats.");

        var booking = new LessonBooking
        {
            LessonId = lesson.Id,
            TeacherId = lesson.TeacherId,
            StudentId = student.Id,
            Status = BookingStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.LessonBookings.Add(booking);
        await dbContext.SaveChangesAsync(cancellationToken);

        var studentName = student.User.FullName;
        var subject = lesson.Subject;

        await notificationService.CreateAsync(
            new NotificationCreateRequest
            {
                RecipientUserIds = [lesson.Teacher.UserId],
                UserTargetId = student.UserId,
                Type = NotificationType.LessonBookingRequested,
                EntityType = NotificationEntityType.Lesson,
                EntityId = lesson.Id,
                TitleAr = "حجز درس جديد",
                TitleEn = "New lesson booking",
                BodyAr = $"الطالب {studentName} حجز درس «{subject}».",
                BodyEn = $"Student {studentName} booked the lesson '{subject}'.",
                IncludeSuperAdmins = true
            },
            cancellationToken);

        var language = requestLanguage.Current;

        return Result<BookingDto>.Success(new BookingDto
        {
            Id = booking.Id,
            LessonId = booking.LessonId,
            TeacherId = booking.TeacherId,
            StudentId = booking.StudentId,
            Status = booking.Status.ToString(),
            Subject = lesson.Subject,
            AcademicYearName = lesson.AcademicYear.Name,
            EducationStageName = LocalizedNames.Pick(
                lesson.EducationStage.NameAr,
                lesson.EducationStage.NameEn,
                language),
            EducationYearName = LocalizedNames.Pick(
                lesson.EducationYear.NameAr,
                lesson.EducationYear.NameEn,
                language),
            StartDate = lesson.StartDate,
            CreatedAtUtc = booking.CreatedAtUtc
        });
    }
}
