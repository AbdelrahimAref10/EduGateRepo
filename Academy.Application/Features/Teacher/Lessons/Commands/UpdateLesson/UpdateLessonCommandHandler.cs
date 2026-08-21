using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Commands.UpdateLesson;

public sealed class UpdateLessonCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<UpdateLessonCommand, Result<LessonDto>>
{
    public async Task<Result<LessonDto>> Handle(
        UpdateLessonCommand request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .Include(x => x.User)
                .ThenInclude(x => x.Area!)
                    .ThenInclude(x => x.City)
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<LessonDto>.NotFound("Teacher profile was not found.");

        if (teacher.User.AreaId is null || teacher.User.Area?.City is null)
            return Result<LessonDto>.Failure("Teacher must have a city assigned before updating a lesson.");

        var teacherCityId = teacher.User.Area.CityId;

        var lesson = await dbContext.Lessons
            .AsTracking()
            .Include(x => x.Groups)
            .Include(x => x.Bookings)
            .FirstOrDefaultAsync(
                x => x.Id == request.LessonId && x.TeacherId == teacher.Id,
                cancellationToken);

        if (lesson is null)
            return Result<LessonDto>.NotFound("Lesson was not found.");

        if (lesson.Groups.Any(g => g.StartedAtUtc.HasValue))
            return Result<LessonDto>.Conflict("لا يمكن تعديل الدرس بعد بدء أول مجموعة.");

        var area = await dbContext.Areas
            .Include(x => x.City)
                .ThenInclude(x => x.Governorate)
            .FirstOrDefaultAsync(
                x => x.Id == request.AreaId && x.IsActive && x.CityId == teacherCityId,
                cancellationToken);

        if (area is null)
            return Result<LessonDto>.Failure("Selected area was not found or does not belong to your city.");

        var resolved = await EducationCurriculum.ResolveSubjectAsync(
            dbContext,
            request.EducationTypeId,
            request.EducationStageId,
            request.EducationYearId,
            request.EducationSubjectId,
            cancellationToken);

        if (!resolved.IsSuccess)
            return Result<LessonDto>.Failure(resolved.Error, resolved.StatusCode);

        var subject = resolved.Value!;
        var year = subject.EducationYear;
        var stage = year.EducationStage;
        var type = stage.EducationType;

        lesson.EducationSubjectId = subject.Id;
        lesson.Subject = LocalizedNames.Pick(subject.NameAr, subject.NameEn, requestLanguage.Current);
        lesson.EducationTypeId = type.Id;
        lesson.EducationStageId = stage.Id;
        lesson.EducationYearId = year.Id;
        lesson.BillingType = request.BillingType;
        lesson.SessionPrice = request.BillingType == BillingType.PerSession ? request.SessionPrice : null;
        lesson.MonthlyPrice = request.BillingType == BillingType.Monthly ? request.MonthlyPrice : null;
        lesson.StartDate = request.StartDate;
        lesson.CountryId = area.City.Governorate.CountryId;
        lesson.AreaId = area.Id;

        await dbContext.SaveChangesAsync(cancellationToken);

        lesson.EducationType = type;
        lesson.EducationStage = stage;
        lesson.EducationYear = year;
        lesson.EducationSubject = subject;
        lesson.Area = area;
        lesson.Country = await dbContext.Countries.FirstAsync(x => x.Id == lesson.CountryId, cancellationToken);

        return Result<LessonDto>.Success(LessonMappings.ToLessonDto(
            lesson,
            lesson.Groups.Count,
            lesson.Bookings.Count,
            lesson.Bookings.Count(b => b.Status == BookingStatus.Confirmed),
            hasStartedGroup: false,
            requestLanguage.Current));
    }
}
