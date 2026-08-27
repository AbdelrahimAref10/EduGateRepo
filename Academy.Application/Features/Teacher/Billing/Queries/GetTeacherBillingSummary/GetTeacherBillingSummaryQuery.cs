using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Common;
using Academy.Application.Features.Teacher.Billing.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingSummary;

public sealed record GetTeacherBillingSummaryQuery(
    int UserId,
    int? StudentId = null,
    int? AcademicYearId = null,
    int? EducationStageId = null,
    int? LessonId = null,
    int? GroupId = null,
    int? SessionId = null)
    : IRequest<Result<TeacherBillingSummaryDto>>;

public sealed class GetTeacherBillingSummaryQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherBillingSummaryQuery, Result<TeacherBillingSummaryDto>>
{
    public async Task<Result<TeacherBillingSummaryDto>> Handle(
        GetTeacherBillingSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await TeacherBillingAccess.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<TeacherBillingSummaryDto>.NotFound("Teacher profile was not found.");

        var (todayFrom, todayTo) = LedgerCalendar.TodayWindow(DateTime.UtcNow);

        var charges = dbContext.Charges
            .AsNoTracking()
            .Where(x => x.TeacherId == teacherId.Value && x.Status != ChargeStatus.Deferred)
            .ApplyChargeFilters(
                request.StudentId,
                request.LessonId,
                request.GroupId,
                fromUtc: null,
                toUtcExclusive: null,
                type: null,
                request.AcademicYearId,
                request.EducationStageId,
                request.SessionId);

        var payments = dbContext.Payments
            .AsNoTracking()
            .Where(x => x.TeacherId == teacherId.Value)
            .ApplyPaymentFilters(
                request.StudentId,
                request.LessonId,
                request.GroupId,
                fromUtc: null,
                toUtcExclusive: null,
                request.AcademicYearId,
                request.EducationStageId,
                request.SessionId);

        var chargeTotals = await charges
            .GroupBy(x => x.TeacherId)
            .Select(g => new
            {
                Total = g.Sum(x => x.Amount),
                Today = g.Sum(x =>
                    x.CreatedAtUtc >= todayFrom && x.CreatedAtUtc < todayTo ? x.Amount : 0m)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var paymentTotals = await payments
            .GroupBy(x => x.TeacherId)
            .Select(g => new
            {
                Total = g.Sum(x => x.Amount),
                Today = g.Sum(x =>
                    x.PaidAtUtc >= todayFrom && x.PaidAtUtc < todayTo ? x.Amount : 0m)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var chargesTotal = chargeTotals?.Total ?? 0m;
        var paymentsTotal = paymentTotals?.Total ?? 0m;
        var todayChargesTotal = chargeTotals?.Today ?? 0m;
        var todayPaymentsTotal = paymentTotals?.Today ?? 0m;

        return Result<TeacherBillingSummaryDto>.Success(new TeacherBillingSummaryDto
        {
            ChargesTotal = chargesTotal,
            PaymentsTotal = paymentsTotal,
            NetOutstanding = chargesTotal - paymentsTotal,
            TodayChargesTotal = todayChargesTotal,
            TodayPaymentsTotal = todayPaymentsTotal,
            TodayNetOutstanding = todayChargesTotal - todayPaymentsTotal
        });
    }
}
