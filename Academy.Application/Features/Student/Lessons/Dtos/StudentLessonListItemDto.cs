namespace Academy.Application.Features.Student.Lessons.Dtos;

public sealed class StudentLessonListItemDto
{
    public required int LessonId { get; init; }

    public required int BookingId { get; init; }

    public required string BookingStatus { get; init; }

    public required string Subject { get; init; }

    public required string TeacherName { get; init; }

    public required string EducationTypeName { get; init; }

    public required string EducationStageName { get; init; }

    public required string EducationYearName { get; init; }

    public required string BillingType { get; init; }

    public decimal? SessionPrice { get; init; }

    public decimal? MonthlyPrice { get; init; }

    public required DateOnly StartDate { get; init; }

    public int? AssignedGroupId { get; init; }

    public string? AssignedGroupName { get; init; }

    public required bool CanEnterLesson { get; init; }

    public DateOnly? NextSessionDate { get; init; }
}
