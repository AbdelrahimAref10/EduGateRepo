namespace Academy.Application.Features.Marketplace.Dtos;

public sealed class PublicLessonCardDto
{
    public required int Id { get; init; }

    public required int TeacherId { get; init; }

    public required string TeacherName { get; init; }

    public string? TeacherPhotoUrl { get; init; }

    public required string Subject { get; init; }

    public required int EducationTypeId { get; init; }

    public required string EducationTypeName { get; init; }

    public required int EducationStageId { get; init; }

    public required string EducationStageName { get; init; }

    public required int EducationYearId { get; init; }

    public required string EducationYearName { get; init; }

    public required string BillingType { get; init; }

    public decimal? SessionPrice { get; init; }

    public decimal? MonthlyPrice { get; init; }

    public required DateOnly StartDate { get; init; }

    public required int CountryId { get; init; }

    public required string CountryName { get; init; }

    public int? RemainingSeats { get; init; }

    public required bool SeatsOpen { get; init; }

    public required bool IsFull { get; init; }

    public required decimal TeacherRatingAverage { get; init; }

    public required int TeacherRatingCount { get; init; }

    public required int TeacherRatingStars { get; init; }

    public required bool AlreadyBooked { get; init; }
}
