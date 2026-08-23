using Academy.Application.Common.Images;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetLessonDebts;

public sealed class GetLessonDebtsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetLessonDebtsQuery, Result<IReadOnlyList<LedgerStudentRowDto>>>
{
    public async Task<Result<IReadOnlyList<LedgerStudentRowDto>>> Handle(
        GetLessonDebtsQuery request,
        CancellationToken cancellationToken)
    {
        var lessonOk = await dbContext.Lessons.AnyAsync(
            x => x.Id == request.LessonId && x.Teacher.UserId == request.UserId,
            cancellationToken);

        if (!lessonOk)
            return Result<IReadOnlyList<LedgerStudentRowDto>>.NotFound("الدرس غير موجود.");

        var raw = await dbContext.Charges
            .AsNoTracking()
            .Where(x =>
                x.LessonId == request.LessonId
                && x.Status != ChargeStatus.Deferred
                && x.Allocations.Sum(a => a.Amount) < x.Amount)
            .GroupBy(x => new
            {
                x.StudentId,
                StudentName = (x.Student.User.FirstName + " " + x.Student.User.LastName).Trim(),
                x.Student.StudentCode,
                Photo = x.Student.User.ProfilePhoto
            })
            .Select(g => new
            {
                g.Key.StudentId,
                g.Key.StudentName,
                g.Key.StudentCode,
                g.Key.Photo,
                OutstandingAmount = g.Sum(c => c.Amount - c.Allocations.Sum(a => a.Amount)),
                OpenChargesCount = g.Count()
            })
            .Where(x => x.OutstandingAmount > 0)
            .OrderByDescending(x => x.OutstandingAmount)
            .ToListAsync(cancellationToken);

        var rows = raw.Select(x => new LedgerStudentRowDto
        {
            StudentId = x.StudentId,
            StudentName = x.StudentName,
            StudentCode = x.StudentCode,
            PhotoUrl = ImageService.DisplayValue(x.Photo),
            OutstandingAmount = x.OutstandingAmount,
            OpenChargesCount = x.OpenChargesCount,
            LastPaymentAtUtc = null,
            LastPaymentAmount = null
        }).ToList();

        return Result<IReadOnlyList<LedgerStudentRowDto>>.Success(rows);
    }
}
