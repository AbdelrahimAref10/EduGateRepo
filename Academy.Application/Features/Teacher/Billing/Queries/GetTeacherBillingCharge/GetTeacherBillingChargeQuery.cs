using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Common;
using Academy.Application.Features.Teacher.Billing.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingCharge;

public sealed record GetTeacherBillingChargeQuery(int UserId, int ChargeId)
    : IRequest<Result<LedgerChargeDetailDto>>;

public sealed class GetTeacherBillingChargeQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherBillingChargeQuery, Result<LedgerChargeDetailDto>>
{
    public async Task<Result<LedgerChargeDetailDto>> Handle(
        GetTeacherBillingChargeQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await TeacherBillingAccess.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<LedgerChargeDetailDto>.NotFound("Teacher profile was not found.");

        var row = await LedgerChargeRows.SelectRows(
                dbContext.Charges
                    .AsNoTracking()
                    .Where(x => x.Id == request.ChargeId && x.TeacherId == teacherId.Value))
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
            return Result<LedgerChargeDetailDto>.NotFound("الفاتورة غير موجودة.");

        var allocations = await dbContext.PaymentAllocations
            .AsNoTracking()
            .Where(x => x.ChargeId == request.ChargeId)
            .OrderByDescending(x => x.Payment.PaidAtUtc)
            .ThenByDescending(x => x.Id)
            .Select(x => new LedgerChargeAllocationDto
            {
                PaymentId = x.PaymentId,
                ReceiptNumber = x.Payment.ReceiptNumber,
                Amount = x.Amount,
                PaidAtUtc = x.Payment.PaidAtUtc,
                Method = x.Payment.Method.ToString()
            })
            .ToListAsync(cancellationToken);

        return Result<LedgerChargeDetailDto>.Success(new LedgerChargeDetailDto
        {
            Charge = LedgerChargeRows.ToDto(row),
            Allocations = allocations
        });
    }
}
