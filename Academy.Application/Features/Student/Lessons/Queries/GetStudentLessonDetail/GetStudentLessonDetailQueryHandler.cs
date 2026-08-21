using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Student.Lessons.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Student.Lessons.Queries.GetStudentLessonDetail;

public sealed class GetStudentLessonDetailQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetStudentLessonDetailQuery, Result<StudentLessonDetailDto>>
{
    public async Task<Result<StudentLessonDetailDto>> Handle(
        GetStudentLessonDetailQuery request,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && !x.IsParent, cancellationToken);

        if (student is null)
            return Result<StudentLessonDetailDto>.NotFound("Student profile was not found.");

        var booking = await dbContext.LessonBookings
            .AsNoTracking()
            .Include(x => x.Lesson)
                .ThenInclude(x => x.Teacher)
                    .ThenInclude(x => x.User)
            .Include(x => x.Lesson)
                .ThenInclude(x => x.EducationType)
            .Include(x => x.Lesson)
                .ThenInclude(x => x.EducationStage)
            .Include(x => x.Lesson)
                .ThenInclude(x => x.EducationYear)
            .Include(x => x.Lesson)
                .ThenInclude(x => x.EducationSubject)
            .Include(x => x.Lesson)
                .ThenInclude(x => x.Country)
            .Include(x => x.Lesson)
                .ThenInclude(x => x.Area)
            .FirstOrDefaultAsync(
                x => x.StudentId == student.Id && x.LessonId == request.LessonId,
                cancellationToken);

        if (booking is null)
            return Result<StudentLessonDetailDto>.NotFound("Lesson booking was not found.");

        var language = requestLanguage.Current;
        var lesson = booking.Lesson;

        var membership = await dbContext.LessonGroupMembers
            .AsNoTracking()
            .Include(x => x.LessonGroup)
                .ThenInclude(x => x.Area)
            .Include(x => x.LessonGroup)
                .ThenInclude(x => x.Sessions)
            .FirstOrDefaultAsync(
                x => x.StudentId == student.Id && x.LessonGroup.LessonId == request.LessonId,
                cancellationToken);

        StudentLessonGroupDto? myGroup = null;
        if (membership is not null)
        {
            var group = membership.LessonGroup;
            myGroup = new StudentLessonGroupDto
            {
                GroupId = group.Id,
                Name = group.Name,
                PeriodStartDate = group.PeriodStartDate,
                PeriodEndDate = group.PeriodEndDate,
                AreaName = LocalizedNames.Pick(group.Area.NameAr, group.Area.NameEn, language),
                Address = group.Address,
                Notes = group.Notes,
                HasStarted = group.StartedAtUtc.HasValue,
                HasEnded = group.EndedAtUtc.HasValue,
                Sessions = group.Sessions
                    .OrderBy(x => x.SessionDate)
                    .ThenBy(x => x.StartTime)
                    .Select(x =>
                    {
                        var hasStarted = x.StartedAtUtc.HasValue;
                        var hasEnded = x.EndedAtUtc.HasValue;
                        return new StudentLessonSessionDto
                        {
                            SessionId = x.Id,
                            LessonGroupId = x.LessonGroupId,
                            SessionDate = x.SessionDate,
                            StartTime = x.StartTime,
                            Topic = x.Topic,
                            HasStarted = hasStarted,
                            HasEnded = hasEnded,
                            CanOpenClassroom = hasStarted
                        };
                    })
                    .ToList()
            };
        }

        return Result<StudentLessonDetailDto>.Success(new StudentLessonDetailDto
        {
            LessonId = lesson.Id,
            BookingId = booking.Id,
            BookingStatus = booking.Status.ToString(),
            Subject = LocalizedNames.Pick(
                lesson.EducationSubject.NameAr,
                lesson.EducationSubject.NameEn,
                language),
            TeacherName = $"{lesson.Teacher.User.FirstName} {lesson.Teacher.User.LastName}".Trim(),
            EducationTypeName = LocalizedNames.Pick(
                lesson.EducationType.NameAr,
                lesson.EducationType.NameEn,
                language),
            EducationStageName = LocalizedNames.Pick(
                lesson.EducationStage.NameAr,
                lesson.EducationStage.NameEn,
                language),
            EducationYearName = LocalizedNames.Pick(
                lesson.EducationYear.NameAr,
                lesson.EducationYear.NameEn,
                language),
            BillingType = lesson.BillingType.ToString(),
            SessionPrice = lesson.SessionPrice,
            MonthlyPrice = lesson.MonthlyPrice,
            StartDate = lesson.StartDate,
            CountryName = LocalizedNames.Pick(lesson.Country.NameAr, lesson.Country.NameEn, language),
            AreaName = LocalizedNames.Pick(lesson.Area.NameAr, lesson.Area.NameEn, language),
            MyGroup = myGroup
        });
    }
}

