using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Common;
using Academy.Application.Features.Teacher.Billing.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingPayment;

public sealed record GetTeacherBillingPaymentQuery(int UserId, int PaymentId)
    : IRequest<Result<LedgerPaymentDetailDto>>;

public sealed class GetTeacherBillingPaymentQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherBillingPaymentQuery, Result<LedgerPaymentDetailDto>>
{
    public async Task<Result<LedgerPaymentDetailDto>> Handle(
        GetTeacherBillingPaymentQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await TeacherBillingAccess.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<LedgerPaymentDetailDto>.NotFound("Teacher profile was not found.");

        var row = await dbContext.Payments
            .AsNoTracking()
            .Where(x => x.Id == request.PaymentId && x.TeacherId == teacherId.Value)
            .Select(x => new
            {
                x.Id,
                x.PaidAtUtc,
                x.StudentId,
                StudentName = (x.Student.User.FirstName + " " + x.Student.User.LastName).Trim(),
                x.Student.StudentCode,
                x.LessonId,
                LessonTitle = x.Lesson.Subject,
                x.Amount,
                x.Method,
                x.ReceiptNumber,
                x.Note,
                Allocations = x.Allocations
                    .OrderBy(a => a.Id)
                    .Select(a => new
                    {
                        a.ChargeId,
                        a.Amount,
                        ChargeType = a.Charge.Type,
                        a.Charge.LessonGroupId,
                        GroupName = a.Charge.LessonGroup != null ? a.Charge.LessonGroup.Name : null
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
            return Result<LedgerPaymentDetailDto>.NotFound("الدفعة غير موجودة.");

        int? groupId = null;
        string? groupName = null;
        if (row.Allocations.Count > 0)
        {
            var firstGroup = row.Allocations[0].LessonGroupId;
            if (row.Allocations.All(a => a.LessonGroupId == firstGroup))
            {
                groupId = firstGroup;
                groupName = row.Allocations[0].GroupName;
            }
        }

        return Result<LedgerPaymentDetailDto>.Success(new LedgerPaymentDetailDto
        {
            Payment = new LedgerPaymentRowDto
            {
                Id = row.Id,
                PaidAtUtc = row.PaidAtUtc,
                StudentId = row.StudentId,
                StudentName = row.StudentName,
                StudentCode = row.StudentCode,
                LessonId = row.LessonId,
                LessonTitle = row.LessonTitle,
                GroupId = groupId,
                GroupName = groupName,
                Amount = row.Amount,
                Method = row.Method.ToString(),
                ReceiptNumber = row.ReceiptNumber,
                Note = row.Note
            },
            Allocations = row.Allocations.Select(a => new PaymentAllocationDto
            {
                ChargeId = a.ChargeId,
                Amount = a.Amount,
                ChargeType = a.ChargeType.ToString()
            }).ToList()
        });
    }
}
