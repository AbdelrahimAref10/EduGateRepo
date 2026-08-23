using Academy.Domain.Common;

namespace Academy.Domain.Entities;

/// <summary>
/// Links a payment to one or more charges (partial allocations allowed).
/// </summary>
public class PaymentAllocation : BaseEntity
{
    public int PaymentId { get; set; }

    public Payment Payment { get; set; } = null!;

    public int ChargeId { get; set; }

    public Charge Charge { get; set; } = null!;

    public decimal Amount { get; set; }
}
