using Academy.Application.Common.Models;
using Academy.Application.Contracts.Billing;
using Academy.Application.Contracts.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetPaymentReceiptPdf;

public sealed class GetPaymentReceiptPdfQueryHandler(
    IApplicationDbContext dbContext,
    IPaymentReceiptPdfGenerator pdfGenerator)
    : IRequestHandler<GetPaymentReceiptPdfQuery, Result<PaymentReceiptFileDto>>
{
    public async Task<Result<PaymentReceiptFileDto>> Handle(
        GetPaymentReceiptPdfQuery request,
        CancellationToken cancellationToken)
    {
        var payment = await dbContext.Payments
            .AsNoTracking()
            .Include(x => x.Teacher).ThenInclude(t => t.User)
            .Include(x => x.Student).ThenInclude(s => s.User)
            .Include(x => x.Lesson)
            .Include(x => x.Allocations).ThenInclude(a => a.Charge)
            .FirstOrDefaultAsync(x => x.Id == request.PaymentId, cancellationToken);

        if (payment is null)
            return Result<PaymentReceiptFileDto>.NotFound("الإيصال غير موجود.");

        if (request.AsStudent)
        {
            var ok = await dbContext.Students.AnyAsync(
                x => x.UserId == request.UserId && x.Id == payment.StudentId && !x.IsParent,
                cancellationToken);

            if (!ok)
                return Result<PaymentReceiptFileDto>.NotFound("الإيصال غير موجود.");
        }
        else
        {
            var ok = await dbContext.Teachers.AnyAsync(
                x => x.UserId == request.UserId && x.Id == payment.TeacherId,
                cancellationToken);

            if (!ok)
                return Result<PaymentReceiptFileDto>.NotFound("الإيصال غير موجود.");
        }

        var bytes = pdfGenerator.Generate(new PaymentReceiptPdfModel
        {
            ReceiptNumber = payment.ReceiptNumber,
            TeacherName = $"{payment.Teacher.User.FirstName} {payment.Teacher.User.LastName}".Trim(),
            StudentName = $"{payment.Student.User.FirstName} {payment.Student.User.LastName}".Trim(),
            StudentCode = payment.Student.StudentCode,
            Subject = payment.Lesson.Subject,
            Amount = payment.Amount,
            Method = payment.Method.ToString(),
            PaidAtUtc = payment.PaidAtUtc,
            Note = payment.Note,
            Allocations = payment.Allocations.Select(a => new PaymentReceiptAllocationLine
            {
                ChargeType = a.Charge.Type.ToString(),
                Amount = a.Amount
            }).ToList()
        });

        return Result<PaymentReceiptFileDto>.Success(new PaymentReceiptFileDto
        {
            Content = bytes,
            FileName = $"receipt-{payment.ReceiptNumber}.pdf"
        });
    }
}
