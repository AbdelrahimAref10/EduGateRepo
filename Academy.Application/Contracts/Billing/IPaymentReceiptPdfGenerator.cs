namespace Academy.Application.Contracts.Billing;

public interface IPaymentReceiptPdfGenerator
{
    byte[] Generate(PaymentReceiptPdfModel model);
}

public sealed class PaymentReceiptPdfModel
{
    public required int ReceiptNumber { get; init; }

    public required string TeacherName { get; init; }

    public required string StudentName { get; init; }

    public string? StudentCode { get; init; }

    public required string Subject { get; init; }

    public required decimal Amount { get; init; }

    public required string Method { get; init; }

    public required DateTime PaidAtUtc { get; init; }

    public string? Note { get; init; }

    public required IReadOnlyList<PaymentReceiptAllocationLine> Allocations { get; init; }
}

public sealed class PaymentReceiptAllocationLine
{
    public required string ChargeType { get; init; }

    public required decimal Amount { get; init; }
}
