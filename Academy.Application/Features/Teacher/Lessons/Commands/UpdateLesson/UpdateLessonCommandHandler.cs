using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
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
            .Include(x => x.EducationType)
            .Include(x => x.EducationYear)
            .Include(x => x.Country)
            .Include(x => x.Area)
                .ThenInclude(x => x.City)
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

        var educationYear = await dbContext.EducationYears
            .Include(x => x.EducationType)
            .FirstOrDefaultAsync(
                x => x.Id == request.EducationYearId
                     && x.IsActive
                     && x.EducationType.IsActive,
                cancellationToken);

        if (educationYear is null)
            return Result<LessonDto>.NotFound("Education year was not found.");

        if (educationYear.EducationTypeId != request.EducationTypeId)
            return Result<LessonDto>.Failure("Education year does not belong to the selected education type.");

        lesson.Subject = request.Subject.Trim();
        lesson.EducationTypeId = educationYear.EducationTypeId;
        lesson.EducationYearId = educationYear.Id;
        lesson.BillingType = request.BillingType;
        lesson.SessionPrice = request.BillingType == BillingType.PerSession ? request.SessionPrice : null;
        lesson.MonthlyPrice = request.BillingType == BillingType.Monthly ? request.MonthlyPrice : null;
        lesson.StartDate = request.StartDate;
        lesson.CountryId = area.City.Governorate.CountryId;
        lesson.AreaId = area.Id;

        await dbContext.SaveChangesAsync(cancellationToken);

        // Reload navigation for response
        lesson.EducationType = educationYear.EducationType;
        lesson.EducationYear = educationYear;
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
