using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Common;
using Academy.Application.Features.Teacher.Billing.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingCharges;

public sealed record GetTeacherBillingChargesQuery(
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
    ChargeStatus? Status = null,
    int Page = 1,
    int PageSize = LedgerPaging.PageSize)
    : IRequest<Result<PagedResult<LedgerChargeRowDto>>>;

public sealed class GetTeacherBillingChargesQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherBillingChargesQuery, Result<PagedResult<LedgerChargeRowDto>>>
{
    public async Task<Result<PagedResult<LedgerChargeRowDto>>> Handle(
        GetTeacherBillingChargesQuery request,
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
            .Where(x => x.TeacherId == teacherId.Value)
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

        if (request.Status is ChargeStatus status)
            query = query.Where(x => x.Status == status);

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
