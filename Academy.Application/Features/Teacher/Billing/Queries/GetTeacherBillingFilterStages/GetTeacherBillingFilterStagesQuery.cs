using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Common;
using Academy.Application.Features.Teacher.Billing.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingFilterStages;

public sealed record GetTeacherBillingFilterStagesQuery(int UserId, int AcademicYearId)
    : IRequest<Result<IReadOnlyList<LedgerFilterOptionDto>>>;

public sealed class GetTeacherBillingFilterStagesQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetTeacherBillingFilterStagesQuery, Result<IReadOnlyList<LedgerFilterOptionDto>>>
{
    public async Task<Result<IReadOnlyList<LedgerFilterOptionDto>>> Handle(
        GetTeacherBillingFilterStagesQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await TeacherBillingAccess.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<IReadOnlyList<LedgerFilterOptionDto>>.NotFound("Teacher profile was not found.");

        var isArabic = requestLanguage.Current == AppLanguage.Arabic;

        var items = await dbContext.Lessons
            .AsNoTracking()
            .Where(x =>
                x.TeacherId == teacherId.Value
                && x.AcademicYearId == request.AcademicYearId)
            .Select(x => x.EducationStage)
            .Distinct()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.NameEn)
            .Select(x => new LedgerFilterOptionDto
            {
                Id = x.Id,
                Name = isArabic ? x.NameAr : x.NameEn
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<LedgerFilterOptionDto>>.Success(items);
    }
}
