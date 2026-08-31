namespace Academy.Application.Features.LearningPath.Dtos;

public sealed class WeeklyLearningPlanDto
{
    public required DateOnly WeekStart { get; init; }

    public required DateOnly WeekEnd { get; init; }

    public required IReadOnlyList<WeeklyPlanSessionDto> Sessions { get; init; }

    public required IReadOnlyList<WeeklyPlanChargeDto> UnpaidCharges { get; init; }

    public required IReadOnlyList<WeeklyPlanExamDto> ExamsDue { get; init; }
}

public sealed class WeeklyPlanSessionDto
{
    public required int SessionId { get; init; }

    public required int LessonId { get; init; }

    public required int StudentId { get; init; }

    public required string StudentName { get; init; }

    public required string Subject { get; init; }

    public required string GroupName { get; init; }

    public required string TeacherName { get; init; }

    public required DateOnly SessionDate { get; init; }

    public required TimeOnly StartTime { get; init; }

    public string? Topic { get; init; }

    public string? Notes { get; init; }

    public required bool HasStarted { get; init; }

    public bool? IsPresent { get; init; }
}

public sealed class WeeklyPlanChargeDto
{
    public required int ChargeId { get; init; }

    public required int StudentId { get; init; }

    public required string StudentName { get; init; }

    public required int LessonId { get; init; }

    public required string Subject { get; init; }

    public required string Type { get; init; }

    public required decimal Remaining { get; init; }
}

public sealed class WeeklyPlanExamDto
{
    public required int ExamId { get; init; }

    public required int SessionId { get; init; }

    public required int StudentId { get; init; }

    public required string StudentName { get; init; }

    public required string Title { get; init; }

    public required string Subject { get; init; }

    public required DateOnly SessionDate { get; init; }

    public required bool SessionStarted { get; init; }

    public required bool HasSubmitted { get; init; }
}

public sealed class ProgressReportDto
{
    public required IReadOnlyList<LessonProgressDto> Lessons { get; init; }
}

public sealed class LessonProgressDto
{
    public required int StudentId { get; init; }

    public required string StudentName { get; init; }

    public required int LessonId { get; init; }

    public required int? GroupId { get; init; }

    public required string Subject { get; init; }

    public required string GroupName { get; init; }

    public required string TeacherName { get; init; }

    public required int SessionsHeld { get; init; }

    public required int SessionsPresent { get; init; }

    public decimal? AttendancePercent { get; init; }

    public required int ExamsTaken { get; init; }

    public decimal? ExamAveragePercent { get; init; }

    public required decimal Outstanding { get; init; }

    public required IReadOnlyList<RecentSessionProgressDto> RecentSessions { get; init; }
}

public sealed class RecentSessionProgressDto
{
    public required int SessionId { get; init; }

    public required DateOnly SessionDate { get; init; }

    public required TimeOnly StartTime { get; init; }

    public string? Topic { get; init; }

    public string? TeacherNotes { get; init; }

    public required bool HasStarted { get; init; }

    public bool? IsPresent { get; init; }
}

public sealed class TeacherGroupProgressDto
{
    public required int LessonId { get; init; }

    public required int GroupId { get; init; }

    public required string Subject { get; init; }

    public required string GroupName { get; init; }

    public required IReadOnlyList<LessonProgressDto> Members { get; init; }
}
