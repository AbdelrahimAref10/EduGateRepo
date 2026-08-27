using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetMyLessons;

public sealed record GetMyLessonsQuery(
    int UserId,
    int? AcademicYearId = null,
    int? EducationStageId = null,
    int? Page = null,
    int? PageSize = null)
    : IRequest<Result<PagedResult<LessonDto>>>;

public sealed class GetMyLessonsQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetMyLessonsQuery, Result<PagedResult<LessonDto>>>
{
    public async Task<Result<PagedResult<LessonDto>>> Handle(
        GetMyLessonsQuery request,
        CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = Paging.Normalize(request.Page, request.PageSize);

        var teacherId = await dbContext.Teachers
            .AsNoTracking()
            .Where(x => x.UserId == request.UserId)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (teacherId is null)
            return Result<PagedResult<LessonDto>>.NotFound("Teacher profile was not found.");

        var isArabic = requestLanguage.Current == AppLanguage.Arabic;

        var query = dbContext.Lessons
            .AsNoTracking()
            .Where(x => x.TeacherId == teacherId.Value);

        if (request.AcademicYearId is int academicYearId && academicYearId > 0)
            query = query.Where(x => x.AcademicYearId == academicYearId);

        if (request.EducationStageId is int stageId && stageId > 0)
            query = query.Where(x => x.EducationStageId == stageId);

        var totalCount = await query.CountAsync(cancellationToken);
        if (totalCount == 0)
            return Result<PagedResult<LessonDto>>.Success(PagedResult<LessonDto>.Empty(page, pageSize));

        var rows = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.TeacherId,
                Subject = isArabic ? x.EducationSubject.NameAr : x.EducationSubject.NameEn,
                x.EducationSubjectId,
                x.AcademicYearId,
                AcademicYearName = x.AcademicYear.Name,
                x.EducationStageId,
                EducationStageName = isArabic ? x.EducationStage.NameAr : x.EducationStage.NameEn,
                x.EducationYearId,
                EducationYearName = isArabic ? x.EducationYear.NameAr : x.EducationYear.NameEn,
                x.BillingType,
                x.SessionPrice,
                x.MonthlyPrice,
                x.ChargeAbsentSessions,
                x.StartDate,
                x.CountryId,
                CountryName = isArabic ? x.Country.NameAr : x.Country.NameEn,
                x.AreaId,
                AreaName = isArabic ? x.Area.NameAr : x.Area.NameEn,
                CityId = x.Area.CityId,
                CityName = isArabic ? x.Area.City.NameAr : x.Area.City.NameEn,
                x.IsActive,
                x.StartedAtUtc,
                x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var ids = rows.Select(x => x.Id).ToList();

        var groupStats = await dbContext.LessonGroups
            .AsNoTracking()
            .Where(x => ids.Contains(x.LessonId))
            .GroupBy(x => x.LessonId)
            .Select(g => new
            {
                LessonId = g.Key,
                Count = g.Count(),
                AnyStarted = g.Any(x => x.StartedAtUtc != null)
            })
            .ToListAsync(cancellationToken);

        var bookingStats = await dbContext.LessonBookings
            .AsNoTracking()
            .Where(x => ids.Contains(x.LessonId))
            .GroupBy(x => x.LessonId)
            .Select(g => new
            {
                LessonId = g.Key,
                Count = g.Count(),
                Confirmed = g.Count(x => x.Status == BookingStatus.Confirmed)
            })
            .ToListAsync(cancellationToken);

        var groupsByLesson = groupStats.ToDictionary(x => x.LessonId);
        var bookingsByLesson = bookingStats.ToDictionary(x => x.LessonId);

        var lessons = rows.Select(x =>
        {
            groupsByLesson.TryGetValue(x.Id, out var groups);
            bookingsByLesson.TryGetValue(x.Id, out var bookings);

            return new LessonDto
            {
                Id = x.Id,
                TeacherId = x.TeacherId,
                Subject = x.Subject,
                EducationSubjectId = x.EducationSubjectId,
                AcademicYearId = x.AcademicYearId,
                AcademicYearName = x.AcademicYearName,
                EducationStageId = x.EducationStageId,
                EducationStageName = x.EducationStageName,
                EducationYearId = x.EducationYearId,
                EducationYearName = x.EducationYearName,
                BillingType = x.BillingType.ToString(),
                SessionPrice = x.SessionPrice,
                MonthlyPrice = x.MonthlyPrice,
                ChargeAbsentSessions = x.ChargeAbsentSessions,
                StartDate = x.StartDate,
                CountryId = x.CountryId,
                CountryName = x.CountryName,
                AreaId = x.AreaId,
                AreaName = x.AreaName,
                CityId = x.CityId,
                CityName = x.CityName,
                IsActive = x.IsActive,
                StartedAtUtc = x.StartedAtUtc,
                HasStarted = x.StartedAtUtc != null,
                CanEdit = groups is null || !groups.AnyStarted,
                GroupsCount = groups?.Count ?? 0,
                BookingsCount = bookings?.Count ?? 0,
                ConfirmedBookingsCount = bookings?.Confirmed ?? 0,
                CreatedAtUtc = x.CreatedAtUtc
            };
        }).ToList();

        return Result<PagedResult<LessonDto>>.Success(
            PagedResult<LessonDto>.Create(lessons, totalCount, page, pageSize));
    }
}
