using Academy.Application.Features.Teacher.Classroom.Dtos;

namespace Academy.Application.Features.SuperAdmin.Groups.Dtos;

public sealed class AdminClassroomDto
{
    public required int SessionId { get; init; }

    public required int LessonId { get; init; }

    public required int LessonGroupId { get; init; }

    public required int SessionNumber { get; init; }

    public required string GroupName { get; init; }

    public required string Subject { get; init; }

    public required DateOnly SessionDate { get; init; }

    public required TimeOnly StartTime { get; init; }

    public string? Topic { get; init; }

    public string? Description { get; init; }

    public required bool HasStarted { get; init; }

    public required bool HasEnded { get; init; }

    public DateTime? StartedAtUtc { get; init; }

    public DateTime? EndedAtUtc { get; init; }

    public required string TeacherName { get; init; }

    public string? TeacherPhotoUrl { get; init; }

    public required string BillingType { get; init; }

    public decimal? SessionPrice { get; init; }

    public decimal? MonthlyPrice { get; init; }

    public required bool HasExam { get; init; }

    public int? ExamStatus { get; init; }

    public string? ExamTitle { get; init; }

    public required int ReviewCount { get; init; }

    public required decimal RatingAverage { get; init; }

    public IReadOnlyList<ClassroomStudentDetailDto> Students { get; init; } = [];

    public IReadOnlyList<ClassroomMaterialDto> Materials { get; init; } = [];
}
