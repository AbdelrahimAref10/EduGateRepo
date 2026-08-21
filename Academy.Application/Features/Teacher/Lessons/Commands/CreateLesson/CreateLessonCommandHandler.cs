using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Commands.CreateLesson;

public sealed class CreateLessonCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<CreateLessonCommand, Result<LessonDto>>
{
    public async Task<Result<LessonDto>> Handle(
        CreateLessonCommand request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .Include(x => x.User)
                .ThenInclude(x => x.Area!)
                    .ThenInclude(x => x.City)
                        .ThenInclude(x => x.Governorate)
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<LessonDto>.NotFound("Teacher profile was not found.");

        if (teacher.User.AreaId is null || teacher.User.Area?.City is null)
            return Result<LessonDto>.Failure("Teacher must have a city assigned before creating a lesson.");

        var teacherCityId = teacher.User.Area.CityId;

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

        var countryId = area.City.Governorate.CountryId;

        var lesson = new Lesson
        {
            TeacherId = teacher.Id,
            Subject = request.Subject.Trim(),
            EducationTypeId = educationYear.EducationTypeId,
            EducationYearId = educationYear.Id,
            BillingType = request.BillingType,
            SessionPrice = request.BillingType == BillingType.PerSession ? request.SessionPrice : null,
            MonthlyPrice = request.BillingType == BillingType.Monthly ? request.MonthlyPrice : null,
            StartDate = request.StartDate,
            CountryId = countryId,
            AreaId = area.Id,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Lessons.Add(lesson);
        await dbContext.SaveChangesAsync(cancellationToken);

        var language = requestLanguage.Current;
        var countryNames = await dbContext.Countries
            .Where(x => x.Id == countryId)
            .Select(x => new { x.NameAr, x.NameEn })
            .FirstAsync(cancellationToken);

        return Result<LessonDto>.Success(new LessonDto
        {
            Id = lesson.Id,
            TeacherId = lesson.TeacherId,
            Subject = lesson.Subject,
            EducationTypeId = educationYear.EducationTypeId,
            EducationTypeName = LocalizedNames.Pick(
                educationYear.EducationType.NameAr,
                educationYear.EducationType.NameEn,
                language),
            EducationYearId = educationYear.Id,
            EducationYearName = LocalizedNames.Pick(
                educationYear.NameAr,
                educationYear.NameEn,
                language),
            BillingType = lesson.BillingType.ToString(),
            SessionPrice = lesson.SessionPrice,
            MonthlyPrice = lesson.MonthlyPrice,
            StartDate = lesson.StartDate,
            CountryId = lesson.CountryId,
            CountryName = LocalizedNames.Pick(countryNames.NameAr, countryNames.NameEn, language),
            AreaId = area.Id,
            AreaName = LocalizedNames.Pick(area.NameAr, area.NameEn, language),
            CityId = area.CityId,
            CityName = LocalizedNames.Pick(area.City.NameAr, area.City.NameEn, language),
            IsActive = lesson.IsActive,
            StartedAtUtc = lesson.StartedAtUtc,
            HasStarted = lesson.StartedAtUtc.HasValue,
            CanEdit = true,
            GroupsCount = 0,
            BookingsCount = 0,
            ConfirmedBookingsCount = 0,
            CreatedAtUtc = lesson.CreatedAtUtc
        });
    }
}
