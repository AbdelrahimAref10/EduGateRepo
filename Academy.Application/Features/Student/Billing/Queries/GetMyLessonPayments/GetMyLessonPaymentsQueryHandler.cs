using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Student.Billing.Queries.GetMyLessonPayments;

public sealed record GetMyLessonPaymentsQuery(int UserId, int LessonId)
    : IRequest<Result<IReadOnlyList<PaymentDto>>>;

public sealed class GetMyLessonPaymentsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetMyLessonPaymentsQuery, Result<IReadOnlyList<PaymentDto>>>
{
    public async Task<Result<IReadOnlyList<PaymentDto>>> Handle(
        GetMyLessonPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && !x.IsParent, cancellationToken);

        if (student is null)
            return Result<IReadOnlyList<PaymentDto>>.NotFound("Student profile was not found.");

        var payments = await dbContext.Payments
            .AsNoTracking()
            .Where(x => x.LessonId == request.LessonId && x.StudentId == student.Id)
            .OrderByDescending(x => x.PaidAtUtc)
            .Select(x => new PaymentDto
            {
                Id = x.Id,
                Amount = x.Amount,
                Method = x.Method.ToString(),
                ReceiptNumber = x.ReceiptNumber,
                PaidAtUtc = x.PaidAtUtc,
                Note = x.Note,
                Allocations = x.Allocations.Select(a => new PaymentAllocationDto
                {
                    ChargeId = a.ChargeId,
                    Amount = a.Amount,
                    ChargeType = a.Charge.Type.ToString()
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<PaymentDto>>.Success(payments);
    }
}
