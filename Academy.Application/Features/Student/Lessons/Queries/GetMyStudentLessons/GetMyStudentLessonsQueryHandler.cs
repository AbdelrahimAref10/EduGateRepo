using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Student.Lessons.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Student.Lessons.Queries.GetMyStudentLessons;

public sealed class GetMyStudentLessonsQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetMyStudentLessonsQuery, Result<IReadOnlyList<StudentLessonListItemDto>>>
{
    public async Task<Result<IReadOnlyList<StudentLessonListItemDto>>> Handle(
        GetMyStudentLessonsQuery request,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && !x.IsParent, cancellationToken);

        if (student is null)
            return Result<IReadOnlyList<StudentLessonListItemDto>>.NotFound("Student profile was not found.");

        var language = requestLanguage.Current;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var bookings = await dbContext.LessonBookings
            .AsNoTracking()
            .Where(x => x.StudentId == student.Id)
            .Include(x => x.Lesson)
                .ThenInclude(x => x.Teacher)
                    .ThenInclude(x => x.User)
            .Include(x => x.Lesson)
                .ThenInclude(x => x.EducationType)
            .Include(x => x.Lesson)
                .ThenInclude(x => x.EducationStage)
            .Include(x => x.Lesson)
                .ThenInclude(x => x.EducationYear)
            .OrderByDescending(x => x.Status == BookingStatus.Confirmed)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var lessonIds = bookings.Select(x => x.LessonId).Distinct().ToList();

        var memberships = await dbContext.LessonGroupMembers
            .AsNoTracking()
            .Where(x => x.StudentId == student.Id && lessonIds.Contains(x.LessonGroup.LessonId))
            .Select(x => new
            {
                x.LessonGroup.LessonId,
                x.LessonGroupId,
                x.LessonGroup.Name
            })
            .ToListAsync(cancellationToken);

        var membershipByLesson = memberships
            .GroupBy(x => x.LessonId)
            .ToDictionary(g => g.Key, g => g.First());

        var groupIds = memberships.Select(x => x.LessonGroupId).Distinct().ToList();

        var nextSessions = await dbContext.LessonGroupSessions
            .AsNoTracking()
            .Where(x => groupIds.Contains(x.LessonGroupId) && x.SessionDate >= today)
            .GroupBy(x => x.LessonGroupId)
            .Select(g => new
            {
                LessonGroupId = g.Key,
                NextDate = g.Min(x => x.SessionDate)
            })
            .ToListAsync(cancellationToken);

        var nextByGroup = nextSessions.ToDictionary(x => x.LessonGroupId, x => x.NextDate);

        var items = bookings.Select(booking =>
        {
            membershipByLesson.TryGetValue(booking.LessonId, out var membership);
            DateOnly? nextSession = null;
            if (membership is not null && nextByGroup.TryGetValue(membership.LessonGroupId, out var next))
                nextSession = next;

            return new StudentLessonListItemDto
            {
                LessonId = booking.LessonId,
                BookingId = booking.Id,
                BookingStatus = booking.Status.ToString(),
                Subject = booking.Lesson.Subject,
                TeacherName = $"{booking.Lesson.Teacher.User.FirstName} {booking.Lesson.Teacher.User.LastName}".Trim(),
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
                BillingType = booking.Lesson.BillingType.ToString(),
                SessionPrice = booking.Lesson.SessionPrice,
                MonthlyPrice = booking.Lesson.MonthlyPrice,
                StartDate = booking.Lesson.StartDate,
                AssignedGroupId = membership?.LessonGroupId,
                AssignedGroupName = membership?.Name,
                CanEnterLesson = booking.Status == BookingStatus.Confirmed,
                NextSessionDate = nextSession
            };
        }).ToList();

        return Result<IReadOnlyList<StudentLessonListItemDto>>.Success(items);
    }
}

