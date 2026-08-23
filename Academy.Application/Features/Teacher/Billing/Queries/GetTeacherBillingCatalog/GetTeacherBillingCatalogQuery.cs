using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingLessons;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingCatalog;

public sealed record GetTeacherBillingCatalogQuery(int UserId)
    : IRequest<Result<BillingCatalogDto>>;

public sealed class BillingCatalogDto
{
    public required decimal TotalOutstanding { get; init; }

    public required int TotalDebtors { get; init; }

    public required int LessonsCount { get; init; }

    public required IReadOnlyList<BillingEducationTypeNodeDto> EducationTypes { get; init; }
}

public sealed class BillingEducationTypeNodeDto
{
    public required int EducationTypeId { get; init; }

    public required string Name { get; init; }

    public required decimal OutstandingAmount { get; init; }

    public required int DebtorsCount { get; init; }

    public required int LessonsCount { get; init; }

    public required IReadOnlyList<BillingStageNodeDto> Stages { get; init; }
}

public sealed class BillingStageNodeDto
{
    public required int EducationStageId { get; init; }

    public required string Name { get; init; }

    public required decimal OutstandingAmount { get; init; }

    public required int DebtorsCount { get; init; }

    public required int LessonsCount { get; init; }

    public required IReadOnlyList<BillingLessonSummaryDto> Lessons { get; init; }
}

public sealed class GetTeacherBillingCatalogQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetTeacherBillingCatalogQuery, Result<BillingCatalogDto>>
{
    public async Task<Result<BillingCatalogDto>> Handle(
        GetTeacherBillingCatalogQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await dbContext.Teachers
            .AsNoTracking()
            .Where(x => x.UserId == request.UserId)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (teacherId is null)
            return Result<BillingCatalogDto>.NotFound("Teacher profile was not found.");

        var language = requestLanguage.Current;
        var isArabic = language == AppLanguage.Arabic;

        // One lean pass: lesson headers only (no students). Empty types never appear.
        var lessons = await dbContext.Lessons
            .AsNoTracking()
            .Where(x => x.TeacherId == teacherId.Value)
            .OrderBy(x => x.EducationType.SortOrder)
            .ThenBy(x => isArabic ? x.EducationType.NameAr : x.EducationType.NameEn)
            .ThenBy(x => x.EducationStage.SortOrder)
            .ThenBy(x => isArabic ? x.EducationStage.NameAr : x.EducationStage.NameEn)
            .ThenBy(x => x.EducationYear.SortOrder)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Select(x => new
            {
                x.Id,
                Subject = isArabic ? x.EducationSubject.NameAr : x.EducationSubject.NameEn,
                BillingType = x.BillingType.ToString(),
                x.SessionPrice,
                x.MonthlyPrice,
                GroupsCount = x.Groups.Count,
                EducationTypeId = x.EducationTypeId,
                EducationTypeName = isArabic ? x.EducationType.NameAr : x.EducationType.NameEn,
                EducationTypeSort = x.EducationType.SortOrder,
                EducationStageId = x.EducationStageId,
                EducationStageName = isArabic ? x.EducationStage.NameAr : x.EducationStage.NameEn,
                EducationStageSort = x.EducationStage.SortOrder,
                EducationYearId = x.EducationYearId,
                EducationYearName = isArabic ? x.EducationYear.NameAr : x.EducationYear.NameEn,
                EducationYearSort = x.EducationYear.SortOrder
            })
            .ToListAsync(cancellationToken);

        if (lessons.Count == 0)
        {
            return Result<BillingCatalogDto>.Success(new BillingCatalogDto
            {
                TotalOutstanding = 0,
                TotalDebtors = 0,
                LessonsCount = 0,
                EducationTypes = []
            });
        }

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

        var types = lessons
            .GroupBy(x => new { x.EducationTypeId, x.EducationTypeName, x.EducationTypeSort })
            .OrderBy(g => g.Key.EducationTypeSort)
            .ThenBy(g => g.Key.EducationTypeName)
            .Select(typeGroup =>
            {
                var stages = typeGroup
                    .GroupBy(x => new { x.EducationStageId, x.EducationStageName, x.EducationStageSort })
                    .OrderBy(g => g.Key.EducationStageSort)
                    .ThenBy(g => g.Key.EducationStageName)
                    .Select(stageGroup =>
                    {
                        var stageLessons = stageGroup
                            .OrderBy(x => x.EducationYearSort)
                            .ThenBy(x => x.EducationYearName)
                            .Select(l =>
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
                            })
                            .ToList();

                        return new BillingStageNodeDto
                        {
                            EducationStageId = stageGroup.Key.EducationStageId,
                            Name = stageGroup.Key.EducationStageName,
                            OutstandingAmount = stageLessons.Sum(x => x.OutstandingAmount),
                            DebtorsCount = stageLessons.Sum(x => x.DebtorsCount),
                            LessonsCount = stageLessons.Count,
                            Lessons = stageLessons
                        };
                    })
                    .ToList();

                return new BillingEducationTypeNodeDto
                {
                    EducationTypeId = typeGroup.Key.EducationTypeId,
                    Name = typeGroup.Key.EducationTypeName,
                    OutstandingAmount = stages.Sum(x => x.OutstandingAmount),
                    DebtorsCount = stages.Sum(x => x.DebtorsCount),
                    LessonsCount = stages.Sum(x => x.LessonsCount),
                    Stages = stages
                };
            })
            .ToList();

        return Result<BillingCatalogDto>.Success(new BillingCatalogDto
        {
            TotalOutstanding = types.Sum(x => x.OutstandingAmount),
            TotalDebtors = types.Sum(x => x.DebtorsCount),
            LessonsCount = lessons.Count,
            EducationTypes = types
        });
    }
}
