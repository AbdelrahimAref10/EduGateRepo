using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Common;
using Academy.Application.Features.Teacher.Billing.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingOutstanding;

public sealed record GetTeacherBillingOutstandingQuery(
    int UserId,
    int? StudentId = null,
    int? AcademicYearId = null,
    int? EducationStageId = null,
    int? LessonId = null,
    int? GroupId = null,
    int? SessionId = null,
    DateOnly? From = null,
    DateOnly? To = null,
    ChargeType? Type = null,
    int Page = 1,
    int PageSize = LedgerPaging.PageSize)
    : IRequest<Result<PagedResult<LedgerChargeRowDto>>>;

public sealed class GetTeacherBillingOutstandingQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherBillingOutstandingQuery, Result<PagedResult<LedgerChargeRowDto>>>
{
    public async Task<Result<PagedResult<LedgerChargeRowDto>>> Handle(
        GetTeacherBillingOutstandingQuery request,
        CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = LedgerPaging.Normalize(request.Page, request.PageSize);

        var teacherId = await TeacherBillingAccess.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<PagedResult<LedgerChargeRowDto>>.NotFound("Teacher profile was not found.");

        var (fromUtc, toUtc) = LedgerCalendar.Range(request.From, request.To);

        var query = dbContext.Charges
            .AsNoTracking()
            .Where(x =>
                x.TeacherId == teacherId.Value
                && x.Status != ChargeStatus.Deferred
                && x.Amount > x.AllocatedAmount)
            .ApplyChargeFilters(
                request.StudentId,
                request.LessonId,
                request.GroupId,
                fromUtc,
                toUtc,
                request.Type,
                request.AcademicYearId,
                request.EducationStageId,
                request.SessionId);

        var totalCount = await query.CountAsync(cancellationToken);
        if (totalCount == 0)
            return Result<PagedResult<LedgerChargeRowDto>>.Success(
                PagedResult<LedgerChargeRowDto>.Empty(page, pageSize));

        var rows = await LedgerChargeRows.SelectRows(
                query
                    .OrderByDescending(x => x.CreatedAtUtc)
                    .ThenByDescending(x => x.Id)
                    .Skip(skip)
                    .Take(pageSize))
            .ToListAsync(cancellationToken);

        var items = rows.Select(LedgerChargeRows.ToDto).ToList();

        return Result<PagedResult<LedgerChargeRowDto>>.Success(
            PagedResult<LedgerChargeRowDto>.Create(items, totalCount, page, pageSize));
    }
}
