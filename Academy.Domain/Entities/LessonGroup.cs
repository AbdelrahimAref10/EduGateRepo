using Academy.Domain.Common;

namespace Academy.Domain.Entities;

/// <summary>
/// A scheduled group under a lesson. Weekly schedule lives in <see cref="Dates"/>;
/// concrete class occurrences live in <see cref="Sessions"/>.
/// </summary>
public class LessonGroup : BaseEntity
{
    public int LessonId { get; set; }

    public Lesson Lesson { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int AreaId { get; set; }

    public Area Area { get; set; } = null!;

    /// <summary>
    /// Free-text meeting place / address written by the teacher.
    /// </summary>
    public string Address { get; set; } = null!;

    public string? Notes { get; set; }

    public int? MaxCapacity { get; set; }

    /// <summary>
    /// Inclusive start of the period used to generate sessions.
    /// </summary>
    public DateOnly PeriodStartDate { get; set; }

    /// <summary>
    /// Inclusive end of the period used to generate sessions.
    /// </summary>
    public DateOnly PeriodEndDate { get; set; }

    public DateTime? StartedAtUtc { get; set; }

    public DateTime? EndedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<LessonGroupDate> Dates { get; set; } = [];

    public ICollection<LessonGroupMember> Members { get; set; } = [];

    public ICollection<LessonGroupSession> Sessions { get; set; } = [];
}
