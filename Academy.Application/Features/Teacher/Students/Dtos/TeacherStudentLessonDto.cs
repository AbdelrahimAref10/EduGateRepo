namespace Academy.Application.Features.Teacher.Students.Dtos;

public sealed class TeacherStudentLessonDto
{
    public required int LessonId { get; init; }

    public required string Subject { get; init; }

    public required string AcademicYearName { get; init; }

    public required string EducationStageName { get; init; }

    public required string EducationYearName { get; init; }

    public required string BillingType { get; init; }

    public required DateOnly StartDate { get; init; }

    public required bool IsActive { get; init; }

    public int? AssignedGroupId { get; init; }

    public string? AssignedGroupName { get; init; }
}
