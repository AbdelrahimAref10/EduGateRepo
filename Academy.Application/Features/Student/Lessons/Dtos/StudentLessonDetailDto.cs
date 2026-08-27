namespace Academy.Application.Features.Student.Lessons.Dtos;

public sealed class StudentLessonDetailDto
{
    public required int LessonId { get; init; }

    public required int BookingId { get; init; }

    public required string BookingStatus { get; init; }

    public required string Subject { get; init; }

    public required int TeacherId { get; init; }

    public required string TeacherName { get; init; }

    public string? TeacherPhotoUrl { get; init; }

    public required string AcademicYearName { get; init; }

    public required string EducationStageName { get; init; }

    public required string EducationYearName { get; init; }

    public required string BillingType { get; init; }

    public decimal? SessionPrice { get; init; }

    public decimal? MonthlyPrice { get; init; }

    public required DateOnly StartDate { get; init; }

    public required string CountryName { get; init; }

    public required string AreaName { get; init; }

    /// <summary>Null when the student is not assigned to a group yet.</summary>
    public StudentLessonGroupDto? MyGroup { get; init; }
}

