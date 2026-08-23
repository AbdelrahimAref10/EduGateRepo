using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetStudentLessonLedger;

public sealed class GetStudentLessonLedgerQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetStudentLessonLedgerQuery, Result<StudentLessonLedgerDto>>
{
    public async Task<Result<StudentLessonLedgerDto>> Handle(
        GetStudentLessonLedgerQuery request,
        CancellationToken cancellationToken)
    {
        var lesson = await dbContext.Lessons
            .AsNoTracking()
            .Where(x => x.Id == request.LessonId && x.Teacher.UserId == request.UserId)
            .Select(x => new { x.Id, x.Subject })
            .FirstOrDefaultAsync(cancellationToken);

        if (lesson is null)
            return Result<StudentLessonLedgerDto>.NotFound("الدرس غير موجود.");

        var student = await dbContext.Students
            .AsNoTracking()
            .Where(x => x.Id == request.StudentId && !x.IsParent)
            .Select(x => new { x.Id, Name = (x.User.FirstName + " " + x.User.LastName).Trim() })
            .FirstOrDefaultAsync(cancellationToken);

        if (student is null)
            return Result<StudentLessonLedgerDto>.NotFound("الطالب غير موجود.");

        var charges = await dbContext.Charges
            .AsNoTracking()
            .Where(x => x.LessonId == lesson.Id && x.StudentId == student.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new ChargeDto
            {
                Id = x.Id,
                Type = x.Type.ToString(),
                Amount = x.Amount,
                AllocatedAmount = x.Allocations.Sum(a => a.Amount),
                Remaining = x.Amount - x.Allocations.Sum(a => a.Amount),
                Status = x.Status == ChargeStatus.Deferred
                    ? nameof(ChargeStatus.Deferred)
                    : (x.Amount - x.Allocations.Sum(a => a.Amount)) <= 0
                        ? nameof(ChargeStatus.Paid)
                        : x.Allocations.Any()
                            ? nameof(ChargeStatus.Partial)
                            : nameof(ChargeStatus.Open),
                Settlement = x.Settlement.ToString(),
                LessonGroupSessionId = x.LessonGroupSessionId,
                CycleStartDate = x.CycleStartDate,
                CycleEndDate = x.CycleEndDate,
                ParentChargeId = x.ParentChargeId,
                Note = x.Note,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var payments = await dbContext.Payments
            .AsNoTracking()
            .Where(x => x.LessonId == lesson.Id && x.StudentId == student.Id)
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

        var outstanding = charges
            .Where(c => c.Status != nameof(ChargeStatus.Deferred))
            .Sum(c => c.Remaining);

        return Result<StudentLessonLedgerDto>.Success(new StudentLessonLedgerDto
        {
            LessonId = lesson.Id,
            StudentId = student.Id,
            StudentName = student.Name,
            Subject = lesson.Subject,
            OutstandingAmount = outstanding,
            Charges = charges,
            Payments = payments
        });
    }
}
