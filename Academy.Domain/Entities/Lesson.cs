using Academy.Domain.Common;
using Academy.Domain.Enums;

namespace Academy.Domain.Entities;

/// <summary>
/// Annual lesson offered by a teacher for an academic year, grade, and subject.
/// </summary>
public class Lesson : BaseEntity
{
    public int TeacherId { get; set; }

    public Teacher Teacher { get; set; } = null!;

    public int AcademicYearId { get; set; }

    public AcademicYear AcademicYear { get; set; } = null!;

    public int EducationStageId { get; set; }

    public EducationStage EducationStage { get; set; } = null!;

    public int EducationSubjectId { get; set; }

    public EducationSubject EducationSubject { get; set; } = null!;

    /// <summary>
    /// Denormalized subject name snapshot for notifications and listings.
    /// </summary>
    public string Subject { get; set; } = null!;

    public int EducationYearId { get; set; }

    public EducationYear EducationYear { get; set; } = null!;

    public BillingType BillingType { get; set; }

    public decimal? SessionPrice { get; set; }

    public decimal? MonthlyPrice { get; set; }

    /// <summary>
    /// When true (PerSession only), marking a student absent still creates a session charge.
    /// </summary>
    public bool ChargeAbsentSessions { get; set; }

    public DateOnly StartDate { get; set; }

    public int CountryId { get; set; }

    public Country Country { get; set; } = null!;

    public int AreaId { get; set; }

    public Area Area { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Set automatically when the first group session is started.
    /// </summary>
    public DateTime? StartedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public decimal RatingAverage { get; set; }

    public int RatingCount { get; set; }

    public ICollection<LessonBooking> Bookings { get; set; } = [];

    public ICollection<LessonGroup> Groups { get; set; } = [];

    public ICollection<LessonReview> Reviews { get; set; } = [];

    public bool IsPerSession => BillingType == BillingType.PerSession;

    public bool IsMonthly => BillingType == BillingType.Monthly;

    public bool ShouldCreateSessionCharge(bool isPresent) =>
        IsPerSession && (isPresent || ChargeAbsentSessions);

    public decimal RequireSessionPrice()
    {
        if (SessionPrice is null or <= 0)
            throw new InvalidOperationException("سعر الحصة غير محدد على الدرس.");

        return SessionPrice.Value;
    }

    public decimal RequireMonthlyPrice()
    {
        if (MonthlyPrice is null or <= 0)
            throw new InvalidOperationException("السعر الشهري غير محدد على الدرس.");

        return MonthlyPrice.Value;
    }

    public void SetChargeAbsentSessions(bool value) =>
        ChargeAbsentSessions = IsPerSession && value;
}
