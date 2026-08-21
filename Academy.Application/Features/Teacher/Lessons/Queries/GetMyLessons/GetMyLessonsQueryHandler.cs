using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetMyLessons;

public sealed class GetMyLessonsQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetMyLessonsQuery, Result<IReadOnlyList<LessonDto>>>
{
    public async Task<Result<IReadOnlyList<LessonDto>>> Handle(
        GetMyLessonsQuery request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<IReadOnlyList<LessonDto>>.NotFound("Teacher profile was not found.");

        var language = requestLanguage.Current;

        var lessons = await dbContext.Lessons
            .Where(x => x.TeacherId == teacher.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new LessonDto
            {
                Id = x.Id,
                TeacherId = x.TeacherId,
                Subject = x.Subject,
                EducationTypeId = x.EducationTypeId,
                EducationTypeName = language == AppLanguage.Arabic
                    ? x.EducationType.NameAr
                    : x.EducationType.NameEn,
                EducationYearId = x.EducationYearId,
                EducationYearName = language == AppLanguage.Arabic
                    ? x.EducationYear.NameAr
                    : x.EducationYear.NameEn,
                BillingType = x.BillingType.ToString(),
                SessionPrice = x.SessionPrice,
                MonthlyPrice = x.MonthlyPrice,
                StartDate = x.StartDate,
                CountryId = x.CountryId,
                CountryName = language == AppLanguage.Arabic
                    ? x.Country.NameAr
                    : x.Country.NameEn,
                AreaId = x.AreaId,
                AreaName = language == AppLanguage.Arabic
                    ? x.Area.NameAr
                    : x.Area.NameEn,
                CityId = x.Area.CityId,
                CityName = language == AppLanguage.Arabic
                    ? x.Area.City.NameAr
                    : x.Area.City.NameEn,
                IsActive = x.IsActive,
                StartedAtUtc = x.StartedAtUtc,
                HasStarted = x.StartedAtUtc != null,
                CanEdit = !x.Groups.Any(g => g.StartedAtUtc != null),
                GroupsCount = x.Groups.Count,
                BookingsCount = x.Bookings.Count,
                ConfirmedBookingsCount = x.Bookings.Count(b => b.Status == BookingStatus.Confirmed),
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<LessonDto>>.Success(lessons);
    }
}
