using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingLessons;

public sealed record GetTeacherBillingLessonsQuery(int UserId, int? Page = null, int? PageSize = null)
    : IRequest<Result<PagedResult<BillingLessonSummaryDto>>>;

public sealed class BillingLessonSummaryDto
{
    public required int LessonId { get; init; }

    public required string Subject { get; init; }

    public required string BillingType { get; init; }

    public decimal? SessionPrice { get; init; }

    public decimal? MonthlyPrice { get; init; }

    public required int GroupsCount { get; init; }

    public required decimal OutstandingAmount { get; init; }

    public required int DebtorsCount { get; init; }

    public int EducationTypeId { get; init; }

    public string EducationTypeName { get; init; } = string.Empty;

    public int EducationStageId { get; init; }

    public string EducationStageName { get; init; } = string.Empty;

    public int EducationYearId { get; init; }

    public string EducationYearName { get; init; } = string.Empty;
}

public sealed class GetTeacherBillingLessonsQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetTeacherBillingLessonsQuery, Result<PagedResult<BillingLessonSummaryDto>>>
{
    public async Task<Result<PagedResult<BillingLessonSummaryDto>>> Handle(
        GetTeacherBillingLessonsQuery request,
        CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = Paging.Normalize(request.Page, request.PageSize);

        var teacherId = await dbContext.Teachers
            .Where(x => x.UserId == request.UserId)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (teacherId is null)
            return Result<PagedResult<BillingLessonSummaryDto>>.NotFound("Teacher profile was not found.");

        var language = requestLanguage.Current;

        var baseQuery = dbContext.Lessons
            .Where(x => x.TeacherId == teacherId.Value);

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        if (totalCount == 0)
            return Result<PagedResult<BillingLessonSummaryDto>>.Success(
                PagedResult<BillingLessonSummaryDto>.Empty(page, pageSize));

        var lessons = await baseQuery
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                Subject = language == AppLanguage.Arabic
                    ? x.EducationSubject.NameAr
                    : x.EducationSubject.NameEn,
                BillingType = x.BillingType.ToString(),
                x.SessionPrice,
                x.MonthlyPrice,
                GroupsCount = x.Groups.Count,
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
                    : x.EducationYear.NameEn
            })
            .ToListAsync(cancellationToken);

        var lessonIds = lessons.Select(x => x.Id).ToList();

        var debtAgg = await dbContext.Charges
            .AsNoTracking()
            .Where(x =>
                lessonIds.Contains(x.LessonId)
                && x.Status != ChargeStatus.Deferred
                && x.Allocations.Sum(a => a.Amount) < x.Amount)
            .GroupBy(x => x.LessonId)
            .Select(g => new
            {
                LessonId = g.Key,
                Outstanding = g.Sum(c => c.Amount - c.Allocations.Sum(a => a.Amount)),
                Debtors = g.Select(c => c.StudentId).Distinct().Count()
            })
            .ToListAsync(cancellationToken);

        var debtMap = debtAgg.ToDictionary(x => x.LessonId);

        var items = lessons.Select(l =>
        {
            debtMap.TryGetValue(l.Id, out var d);
            return new BillingLessonSummaryDto
            {
                LessonId = l.Id,
                Subject = l.Subject,
                BillingType = l.BillingType,
                SessionPrice = l.SessionPrice,
                MonthlyPrice = l.MonthlyPrice,
                GroupsCount = l.GroupsCount,
                OutstandingAmount = d?.Outstanding ?? 0,
                DebtorsCount = d?.Debtors ?? 0,
                EducationTypeId = l.EducationTypeId,
                EducationTypeName = l.EducationTypeName,
                EducationStageId = l.EducationStageId,
                EducationStageName = l.EducationStageName,
                EducationYearId = l.EducationYearId,
                EducationYearName = l.EducationYearName
            };
        }).ToList();

        return Result<PagedResult<BillingLessonSummaryDto>>.Success(
            PagedResult<BillingLessonSummaryDto>.Create(items, totalCount, page, pageSize));
    }
}
