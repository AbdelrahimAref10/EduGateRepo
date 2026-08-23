using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetLessonManage;

public sealed class GetLessonManageQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetLessonManageQuery, Result<LessonManageDto>>
{
    public async Task<Result<LessonManageDto>> Handle(
        GetLessonManageQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await dbContext.Teachers
            .AsNoTracking()
            .Where(x => x.UserId == request.UserId)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (teacherId is null)
            return Result<LessonManageDto>.NotFound("Teacher profile was not found.");

        var isArabic = requestLanguage.Current == AppLanguage.Arabic;

        var lesson = await dbContext.Lessons
            .AsNoTracking()
            .Where(x => x.Id == request.LessonId && x.TeacherId == teacherId)
            .Select(x => new LessonDto
            {
                Id = x.Id,
                TeacherId = x.TeacherId,
                Subject = isArabic ? x.EducationSubject.NameAr : x.EducationSubject.NameEn,
                EducationSubjectId = x.EducationSubjectId,
                EducationTypeId = x.EducationTypeId,
                EducationTypeName = isArabic ? x.EducationType.NameAr : x.EducationType.NameEn,
                EducationStageId = x.EducationStageId,
                EducationStageName = isArabic ? x.EducationStage.NameAr : x.EducationStage.NameEn,
                EducationYearId = x.EducationYearId,
                EducationYearName = isArabic ? x.EducationYear.NameAr : x.EducationYear.NameEn,
                BillingType = x.BillingType.ToString(),
                SessionPrice = x.SessionPrice,
                MonthlyPrice = x.MonthlyPrice,
                StartDate = x.StartDate,
                CountryId = x.CountryId,
                CountryName = isArabic ? x.Country.NameAr : x.Country.NameEn,
                AreaId = x.AreaId,
                AreaName = isArabic ? x.Area.NameAr : x.Area.NameEn,
                CityId = x.Area.CityId,
                CityName = isArabic ? x.Area.City.NameAr : x.Area.City.NameEn,
                IsActive = x.IsActive,
                StartedAtUtc = x.StartedAtUtc,
                HasStarted = x.StartedAtUtc != null,
                CanEdit = !x.Groups.Any(g => g.StartedAtUtc != null),
                GroupsCount = x.Groups.Count,
                BookingsCount = x.Bookings.Count,
                ConfirmedBookingsCount = x.Bookings.Count(b => b.Status == BookingStatus.Confirmed),
                CreatedAtUtc = x.CreatedAtUtc
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (lesson is null)
            return Result<LessonManageDto>.NotFound("Lesson was not found.");

        return Result<LessonManageDto>.Success(new LessonManageDto
        {
            Lesson = lesson,
            Students = [],
            Groups = []
        });
    }
}
