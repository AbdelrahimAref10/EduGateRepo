namespace Academy.Application.Features.Teacher.Billing.Dtos;

public sealed class LedgerStudentRowDto
{
    public required int StudentId { get; init; }

    public required string StudentName { get; init; }

    public string? StudentCode { get; init; }

    public string? PhotoUrl { get; init; }

    public required decimal OutstandingAmount { get; init; }

    public required int OpenChargesCount { get; init; }

    public DateTime? LastPaymentAtUtc { get; init; }

    public decimal? LastPaymentAmount { get; init; }
}

public sealed class ChargeDto
{
    public required int Id { get; init; }

    public required string Type { get; init; }

    public required decimal Amount { get; init; }

    public required decimal AllocatedAmount { get; init; }

    public required decimal Remaining { get; init; }

    public required string Status { get; init; }

    public required string Settlement { get; init; }

    public int? LessonGroupSessionId { get; init; }

    public DateOnly? CycleStartDate { get; init; }

    public DateOnly? CycleEndDate { get; init; }

    public int? ParentChargeId { get; init; }

    public string? Note { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}

public sealed class PaymentDto
{
    public required int Id { get; init; }

    public required decimal Amount { get; init; }

    public required string Method { get; init; }

    public required int ReceiptNumber { get; init; }

    public required DateTime PaidAtUtc { get; init; }

    public string? Note { get; init; }

    public required IReadOnlyList<PaymentAllocationDto> Allocations { get; init; }
}

public sealed class PaymentAllocationDto
{
    public required int ChargeId { get; init; }

    public required decimal Amount { get; init; }

    public required string ChargeType { get; init; }
}

public sealed class StudentLessonLedgerDto
{
    public required int LessonId { get; init; }

    public required int StudentId { get; init; }

    public required string StudentName { get; init; }

    public required string Subject { get; init; }

    public required decimal OutstandingAmount { get; init; }

    public required IReadOnlyList<ChargeDto> Charges { get; init; }

    public required IReadOnlyList<PaymentDto> Payments { get; init; }
}

public sealed class RecordPaymentRequest
{
    public int StudentId { get; set; }

    public decimal Amount { get; set; }

    public Domain.Enums.PaymentMethod Method { get; set; }

    public string? Note { get; set; }

    /// <summary>Optional explicit charge ids; otherwise FIFO on open charges.</summary>
    public List<int>? ChargeIds { get; set; }

    public DateTime? PaidAtUtc { get; set; }
}

public sealed class CreateMakeupSessionRequest
{
    public DateOnly SessionDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public string? Topic { get; set; }

    public int? MakeupForSessionId { get; set; }

    public required List<int> StudentIds { get; set; }

    public bool IsFree { get; set; } = true;

    public decimal? Amount { get; set; }

    public Domain.Enums.ChargeSettlement Settlement { get; set; } =
        Domain.Enums.ChargeSettlement.None;
}
