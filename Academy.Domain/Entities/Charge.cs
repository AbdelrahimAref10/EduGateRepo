using Academy.Domain.Common;
using Academy.Domain.Enums;

namespace Academy.Domain.Entities;

/// <summary>
/// Debit / amount owed by a student for a lesson (session, monthly cycle, makeup, or adjustment).
/// </summary>
public class Charge : BaseEntity
{
    public int TeacherId { get; set; }

    public Teacher Teacher { get; set; } = null!;

    public int StudentId { get; set; }

    public Student Student { get; set; } = null!;

    public int LessonId { get; set; }

    public Lesson Lesson { get; set; } = null!;

    public int? LessonGroupId { get; set; }

    public LessonGroup? LessonGroup { get; set; }

    public ChargeType Type { get; set; }

    public decimal Amount { get; set; }

    public decimal AllocatedAmount { get; set; }

    public ChargeStatus Status { get; set; } = ChargeStatus.Open;

    public int? LessonGroupSessionId { get; set; }

    public LessonGroupSession? LessonGroupSession { get; set; }

    public DateOnly? CycleStartDate { get; set; }

    public DateOnly? CycleEndDate { get; set; }

    public ChargeSettlement Settlement { get; set; } = ChargeSettlement.None;

    public int? ParentChargeId { get; set; }

    public Charge? ParentCharge { get; set; }

    public ICollection<Charge> ChildCharges { get; set; } = [];

    public string? Note { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<PaymentAllocation> Allocations { get; set; } = [];

    public decimal Remaining => Amount - AllocatedAmount;

    public bool HasAllocations => AllocatedAmount > 0;

    public bool CanBeRemoved => AllocatedAmount <= 0 && Status != ChargeStatus.Deferred;

    public bool IsOpenForPayment =>
        Status != ChargeStatus.Deferred
        && Status != ChargeStatus.Paid
        && Remaining > 0;

    public bool CoversDate(DateOnly date) =>
        Type == ChargeType.MonthlyCycle
        && CycleStartDate is not null
        && CycleEndDate is not null
        && CycleStartDate <= date
        && CycleEndDate >= date;

    public void RecalculateStatus()
    {
        if (Status == ChargeStatus.Deferred)
            return;

        if (AllocatedAmount <= 0)
            Status = ChargeStatus.Open;
        else if (AllocatedAmount >= Amount)
            Status = ChargeStatus.Paid;
        else
            Status = ChargeStatus.Partial;
    }

    public void ApplyAllocation(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Allocation amount must be positive.");

        if (amount > Remaining)
            throw new InvalidOperationException("Allocation exceeds remaining charge amount.");

        AllocatedAmount += amount;
        RecalculateStatus();
    }

    public void ActivateDeferredAgainstCycle(Charge monthlyCycle)
    {
        if (Status != ChargeStatus.Deferred || Type != ChargeType.Makeup)
            throw new InvalidOperationException("Only deferred makeup charges can be activated.");

        if (monthlyCycle.Type != ChargeType.MonthlyCycle)
            throw new InvalidOperationException("Parent must be a monthly cycle charge.");

        Status = ChargeStatus.Open;
        ParentChargeId = monthlyCycle.Id;
        Settlement = ChargeSettlement.CurrentCycle;
        RecalculateStatus();
    }

    public static Charge CreateSessionCharge(
        Lesson lesson,
        LessonGroupSession session,
        int studentId,
        int createdByUserId)
    {
        var price = lesson.RequireSessionPrice();

        return new Charge
        {
            TeacherId = lesson.TeacherId,
            StudentId = studentId,
            LessonId = lesson.Id,
            LessonGroupId = session.LessonGroupId,
            Type = ChargeType.Session,
            Amount = price,
            AllocatedAmount = 0,
            Status = ChargeStatus.Open,
            LessonGroupSessionId = session.Id,
            Settlement = ChargeSettlement.Standalone,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static Charge CreateMonthlyCycle(
        Lesson lesson,
        LessonGroupSession session,
        int studentId,
        int createdByUserId)
    {
        var price = lesson.RequireMonthlyPrice();
        var start = session.SessionDate;

        return new Charge
        {
            TeacherId = lesson.TeacherId,
            StudentId = studentId,
            LessonId = lesson.Id,
            LessonGroupId = session.LessonGroupId,
            Type = ChargeType.MonthlyCycle,
            Amount = price,
            AllocatedAmount = 0,
            Status = ChargeStatus.Open,
            CycleStartDate = start,
            CycleEndDate = start.AddDays(30),
            Settlement = ChargeSettlement.None,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static Charge CreateMakeupCharge(
        Lesson lesson,
        LessonGroupSession makeupSession,
        int studentId,
        decimal amount,
        ChargeSettlement settlement,
        Charge? currentCycle,
        int createdByUserId)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Makeup amount must be positive.");

        var effectiveSettlement = settlement;
        var status = ChargeStatus.Open;
        int? parentId = null;

        if (lesson.BillingType == BillingType.PerSession)
        {
            effectiveSettlement = ChargeSettlement.Standalone;
        }
        else if (settlement == ChargeSettlement.CurrentCycle)
        {
            if (currentCycle is not null)
            {
                parentId = currentCycle.Id;
                effectiveSettlement = ChargeSettlement.CurrentCycle;
            }
            else
            {
                effectiveSettlement = ChargeSettlement.Standalone;
            }
        }
        else if (settlement == ChargeSettlement.NextCycle)
        {
            status = ChargeStatus.Deferred;
            effectiveSettlement = ChargeSettlement.NextCycle;
        }
        else if (settlement == ChargeSettlement.None)
        {
            effectiveSettlement = ChargeSettlement.Standalone;
        }

        return new Charge
        {
            TeacherId = lesson.TeacherId,
            StudentId = studentId,
            LessonId = lesson.Id,
            LessonGroupId = makeupSession.LessonGroupId,
            Type = ChargeType.Makeup,
            Amount = amount,
            AllocatedAmount = 0,
            Status = status,
            LessonGroupSessionId = makeupSession.Id,
            Settlement = effectiveSettlement,
            ParentChargeId = parentId,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Summarizes open remaining for classroom / ledger display.
    /// </summary>
    public static (decimal Outstanding, string Status) Summarize(IEnumerable<Charge> charges)
    {
        var list = charges
            .Where(c => c.Status != ChargeStatus.Deferred)
            .ToList();

        if (list.Count == 0)
            return (0, "None");

        var outstanding = list.Sum(c => c.Remaining);
        if (outstanding <= 0)
            return (0, ChargeStatus.Paid.ToString());

        if (list.Any(c => c.Status == ChargeStatus.Partial || c.HasAllocations))
            return (outstanding, ChargeStatus.Partial.ToString());

        return (outstanding, ChargeStatus.Open.ToString());
    }
}
