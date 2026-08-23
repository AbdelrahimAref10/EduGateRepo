using Academy.Domain.Common;
using Academy.Domain.Enums;

namespace Academy.Domain.Entities;

/// <summary>
/// Credit / money received from a student for a lesson (recorded by the teacher).
/// </summary>
public class Payment : BaseEntity
{
    public int TeacherId { get; set; }

    public Teacher Teacher { get; set; } = null!;

    public int StudentId { get; set; }

    public Student Student { get; set; } = null!;

    public int LessonId { get; set; }

    public Lesson Lesson { get; set; } = null!;

    public decimal Amount { get; set; }

    public PaymentMethod Method { get; set; }

    public DateTime PaidAtUtc { get; set; }

    /// <summary>Per-teacher sequential receipt number.</summary>
    public int ReceiptNumber { get; set; }

    public string? Note { get; set; }

    public int RecordedByUserId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<PaymentAllocation> Allocations { get; set; } = [];

    public static Payment Create(
        int teacherId,
        int studentId,
        int lessonId,
        decimal amount,
        PaymentMethod method,
        int receiptNumber,
        int recordedByUserId,
        string? note,
        DateTime? paidAtUtc = null)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Payment amount must be positive.");

        if (receiptNumber <= 0)
            throw new InvalidOperationException("Receipt number must be positive.");

        return new Payment
        {
            TeacherId = teacherId,
            StudentId = studentId,
            LessonId = lessonId,
            Amount = amount,
            Method = method,
            PaidAtUtc = paidAtUtc ?? DateTime.UtcNow,
            ReceiptNumber = receiptNumber,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            RecordedByUserId = recordedByUserId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// FIFO allocation across open charges. Returns created allocation rows.
    /// </summary>
    public IReadOnlyList<PaymentAllocation> AllocateFifo(IReadOnlyList<Charge> openCharges)
    {
        var totalOpen = openCharges.Sum(c => c.Remaining);
        if (Amount > totalOpen)
            throw new InvalidOperationException($"Payment exceeds open remaining ({totalOpen:0.##}).");

        var remaining = Amount;
        var created = new List<PaymentAllocation>();

        foreach (var charge in openCharges.OrderBy(c => c.CreatedAtUtc).ThenBy(c => c.Id))
        {
            if (remaining <= 0)
                break;

            if (!charge.IsOpenForPayment)
                continue;

            var take = Math.Min(charge.Remaining, remaining);
            charge.ApplyAllocation(take);

            // Use FK only — setting Charge navigation can make EF re-insert the tracked charge with its Id.
            var allocation = new PaymentAllocation
            {
                Payment = this,
                ChargeId = charge.Id,
                Amount = take
            };
            Allocations.Add(allocation);
            created.Add(allocation);
            remaining -= take;
        }

        if (remaining > 0)
            throw new InvalidOperationException("Could not fully allocate payment.");

        return created;
    }
}
