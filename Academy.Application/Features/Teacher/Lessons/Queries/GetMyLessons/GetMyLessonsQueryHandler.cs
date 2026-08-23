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
    int? EducationTypeId = null,
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

        var language = requestLanguage.Current;

        var query = dbContext.Lessons
            .AsNoTracking()
            .Where(x => x.TeacherId == teacherId.Value);

        if (request.EducationTypeId is int typeId && typeId > 0)
            query = query.Where(x => x.EducationTypeId == typeId);

        var totalCount = await query.CountAsync(cancellationToken);
        if (totalCount == 0)
            return Result<PagedResult<LessonDto>>.Success(PagedResult<LessonDto>.Empty(page, pageSize));

        var lessons = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new LessonDto
            {
                Id = x.Id,
                TeacherId = x.TeacherId,
                Subject = language == AppLanguage.Arabic
                    ? x.EducationSubject.NameAr
                    : x.EducationSubject.NameEn,
                EducationSubjectId = x.EducationSubjectId,
                EducationTypeId = x.EducationTypeId,
                EducationTypeName = language == AppLanguage.Arabic
                    ? x.EducationType.NameAr
                    : x.EducationType.NameEn,
                EducationStageId = x.EducationStageId,
                EducationStageName = language == AppLanguage.Arabic
                    ? x.EducationStage.NameAr
                    : x.EducationStage.NameEn,
                EducationYearId = x.EducationYearId,
                EducationYearName = language == AppLanguage.Arabic
                    ? x.EducationYear.NameAr
                    : x.EducationYear.NameEn,
                BillingType = x.BillingType.ToString(),
                SessionPrice = x.SessionPrice,
                MonthlyPrice = x.MonthlyPrice,
                ChargeAbsentSessions = x.ChargeAbsentSessions,
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

        return Result<PagedResult<LessonDto>>.Success(
            PagedResult<LessonDto>.Create(lessons, totalCount, page, pageSize));
    }
}
