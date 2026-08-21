using Academy.Domain.Common;
using Academy.Domain.Enums;

namespace Academy.Domain.Entities;

/// <summary>
/// Annual lesson offered by a teacher for a subject, stage, and education year.
/// </summary>
public class Lesson : BaseEntity
{
    public int TeacherId { get; set; }

    public Teacher Teacher { get; set; } = null!;

    public int EducationSubjectId { get; set; }

    public EducationSubject EducationSubject { get; set; } = null!;

    /// <summary>
    /// Denormalized subject name snapshot for notifications and listings.
    /// </summary>
    public string Subject { get; set; } = null!;

    public int EducationTypeId { get; set; }

    public EducationType EducationType { get; set; } = null!;

    public int EducationStageId { get; set; }

    public EducationStage EducationStage { get; set; } = null!;

    public int EducationYearId { get; set; }

    public EducationYear EducationYear { get; set; } = null!;

    public BillingType BillingType { get; set; }

    public decimal? SessionPrice { get; set; }

    public decimal? MonthlyPrice { get; set; }

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

    public ICollection<LessonBooking> Bookings { get; set; } = [];

    public ICollection<LessonGroup> Groups { get; set; } = [];
}
