using Academy.Application.Common.Models;
using Academy.Application.Contracts.Billing;
using MediatR;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetPaymentReceiptPdf;

public sealed record GetPaymentReceiptPdfQuery(
    int UserId,
    int PaymentId,
    bool AsStudent) : IRequest<Result<PaymentReceiptFileDto>>;

public sealed class PaymentReceiptFileDto
{
    public required byte[] Content { get; init; }

    public required string FileName { get; init; }

    public string ContentType => "application/pdf";
}
