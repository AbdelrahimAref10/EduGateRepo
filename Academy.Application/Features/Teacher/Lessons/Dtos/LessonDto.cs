namespace Academy.Application.Features.Teacher.Lessons.Dtos;

public sealed class LessonDto
{
    public required int Id { get; init; }

    public required int TeacherId { get; init; }

    public required string Subject { get; init; }

    public required int EducationSubjectId { get; init; }

    public required int AcademicYearId { get; init; }

    public required string AcademicYearName { get; init; }

    public required int EducationStageId { get; init; }

    public required string EducationStageName { get; init; }

    public required int EducationYearId { get; init; }

    public required string EducationYearName { get; init; }

    public required string BillingType { get; init; }

    public decimal? SessionPrice { get; init; }

    public decimal? MonthlyPrice { get; init; }

    public required bool ChargeAbsentSessions { get; init; }

    public required DateOnly StartDate { get; init; }

    public required int CountryId { get; init; }

    public required string CountryName { get; init; }

    public required int AreaId { get; init; }

    public required string AreaName { get; init; }

    public required int CityId { get; init; }

    public required string CityName { get; init; }

    public required bool IsActive { get; init; }

    public DateTime? StartedAtUtc { get; init; }

    public required bool HasStarted { get; init; }

    /// <summary>
    /// True until the first group is started — lesson details can still be edited.
    /// </summary>
    public required bool CanEdit { get; init; }

    public required int GroupsCount { get; init; }

    public required int BookingsCount { get; init; }

    public required int ConfirmedBookingsCount { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
