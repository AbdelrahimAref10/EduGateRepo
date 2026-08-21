using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons;
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
        var countryId = area.City.Governorate.CountryId;
        var language = requestLanguage.Current;

        var lesson = new Lesson
        {
            TeacherId = teacher.Id,
            EducationSubjectId = subject.Id,
            Subject = LocalizedNames.Pick(subject.NameAr, subject.NameEn, language),
            EducationTypeId = type.Id,
            EducationStageId = stage.Id,
            EducationYearId = year.Id,
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

        var countryNames = await dbContext.Countries
            .Where(x => x.Id == countryId)
            .Select(x => new { x.NameAr, x.NameEn })
            .FirstAsync(cancellationToken);

        return Result<LessonDto>.Success(new LessonDto
        {
            Id = lesson.Id,
            TeacherId = lesson.TeacherId,
            Subject = LocalizedNames.Pick(subject.NameAr, subject.NameEn, language),
            EducationSubjectId = subject.Id,
            EducationTypeId = type.Id,
            EducationTypeName = LocalizedNames.Pick(type.NameAr, type.NameEn, language),
            EducationStageId = stage.Id,
            EducationStageName = LocalizedNames.Pick(stage.NameAr, stage.NameEn, language),
            EducationYearId = year.Id,
            EducationYearName = LocalizedNames.Pick(year.NameAr, year.NameEn, language),
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
