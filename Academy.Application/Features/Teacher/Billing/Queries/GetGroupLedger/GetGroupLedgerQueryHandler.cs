using Academy.Application.Common.Images;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetGroupLedger;

public sealed class GetGroupLedgerQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetGroupLedgerQuery, Result<IReadOnlyList<LedgerStudentRowDto>>>
{
    public async Task<Result<IReadOnlyList<LedgerStudentRowDto>>> Handle(
        GetGroupLedgerQuery request,
        CancellationToken cancellationToken)
    {
        var group = await dbContext.LessonGroups
            .AsNoTracking()
            .Where(x =>
                x.Id == request.GroupId
                && x.LessonId == request.LessonId
                && x.Lesson.Teacher.UserId == request.UserId)
            .Select(x => new { x.Id, x.LessonId })
            .FirstOrDefaultAsync(cancellationToken);

        if (group is null)
            return Result<IReadOnlyList<LedgerStudentRowDto>>.NotFound("المجموعة غير موجودة.");

        var members = await dbContext.LessonGroupMembers
            .AsNoTracking()
            .Where(x => x.LessonGroupId == group.Id)
            .OrderBy(x => x.AddedAtUtc)
            .Select(x => new
            {
                x.StudentId,
                StudentName = (x.Student.User.FirstName + " " + x.Student.User.LastName).Trim(),
                x.Student.StudentCode,
                Photo = x.Student.User.ProfilePhoto
            })
            .ToListAsync(cancellationToken);

        var studentIds = members.Select(m => m.StudentId).ToList();

        var chargeAgg = await dbContext.Charges
            .AsNoTracking()
            .Where(x =>
                x.LessonId == group.LessonId
                && studentIds.Contains(x.StudentId)
                && x.Status != ChargeStatus.Deferred)
            .GroupBy(x => x.StudentId)
            .Select(g => new
            {
                StudentId = g.Key,
                Outstanding = g.Sum(c => c.Amount - c.Allocations.Sum(a => a.Amount)),
                OpenCount = g.Count(c =>
                    c.Status != ChargeStatus.Paid
                    && c.Allocations.Sum(a => a.Amount) < c.Amount)
            })
            .ToListAsync(cancellationToken);

        var lastPayments = await dbContext.Payments
            .AsNoTracking()
            .Where(x => x.LessonId == group.LessonId && studentIds.Contains(x.StudentId))
            .GroupBy(x => x.StudentId)
            .Select(g => new
            {
                StudentId = g.Key,
                Last = g.OrderByDescending(p => p.PaidAtUtc).Select(p => new { p.PaidAtUtc, p.Amount }).First()
            })
            .ToListAsync(cancellationToken);

        var chargeMap = chargeAgg.ToDictionary(x => x.StudentId);
        var payMap = lastPayments.ToDictionary(x => x.StudentId);

        var rows = members.Select(m =>
        {
            chargeMap.TryGetValue(m.StudentId, out var c);
            payMap.TryGetValue(m.StudentId, out var p);
            return new LedgerStudentRowDto
            {
                StudentId = m.StudentId,
                StudentName = m.StudentName,
                StudentCode = m.StudentCode,
                PhotoUrl = ImageService.DisplayValue(m.Photo),
                OutstandingAmount = c?.Outstanding ?? 0,
                OpenChargesCount = c?.OpenCount ?? 0,
                LastPaymentAtUtc = p?.Last.PaidAtUtc,
                LastPaymentAmount = p?.Last.Amount
            };
        }).ToList();

        return Result<IReadOnlyList<LedgerStudentRowDto>>.Success(rows);
    }
}
