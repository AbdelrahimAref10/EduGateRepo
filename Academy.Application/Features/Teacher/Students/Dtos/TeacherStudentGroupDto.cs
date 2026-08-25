namespace Academy.Application.Features.Teacher.Students.Dtos;

public sealed class TeacherStudentGroupDto
{
    public required int Id { get; init; }

    public required int LessonId { get; init; }

    public required string Name { get; init; }

    public required IReadOnlyList<TeacherStudentGroupDateDto> Dates { get; init; }

    public required DateOnly PeriodStartDate { get; init; }

    public required DateOnly PeriodEndDate { get; init; }

    public required int AreaId { get; init; }

    public required string AreaName { get; init; }

    public required int CityId { get; init; }

    public required string CityName { get; init; }

    public required string Address { get; init; }

    public string? Notes { get; init; }

    public int? MaxCapacity { get; init; }

    public required int MembersCount { get; init; }

    public int? RemainingCapacity { get; init; }

    public required bool IsFull { get; init; }

    public required bool IsEmpty { get; init; }

    public required bool IsCurrentStudentGroup { get; init; }

    public DateTime? StartedAtUtc { get; init; }

    public DateTime? EndedAtUtc { get; init; }

    public required bool HasStarted { get; init; }

    public required bool HasEnded { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
