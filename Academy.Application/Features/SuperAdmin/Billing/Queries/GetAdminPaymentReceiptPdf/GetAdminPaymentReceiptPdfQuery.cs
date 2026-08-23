using Academy.Application.Common.Models;
using Academy.Application.Contracts.Billing;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Queries.GetPaymentReceiptPdf;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Billing.Queries.GetAdminPaymentReceiptPdf;

public sealed record GetAdminPaymentReceiptPdfQuery(int PaymentId)
    : IRequest<Result<PaymentReceiptFileDto>>;

public sealed class GetAdminPaymentReceiptPdfQueryHandler(
    IApplicationDbContext dbContext,
    IPaymentReceiptPdfGenerator pdfGenerator)
    : IRequestHandler<GetAdminPaymentReceiptPdfQuery, Result<PaymentReceiptFileDto>>
{
    public async Task<Result<PaymentReceiptFileDto>> Handle(
        GetAdminPaymentReceiptPdfQuery request,
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

        var bytes = pdfGenerator.Generate(new PaymentReceiptPdfModel
        {
            ReceiptNumber = payment.ReceiptNumber,
            TeacherName = payment.Teacher.User.FullName,
            StudentName = payment.Student.User.FullName,
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
