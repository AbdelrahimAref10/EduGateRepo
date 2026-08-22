namespace Academy.Application.Features.Reviews.Dtos;

public sealed class TeacherReviewInboxItemDto
{
    public int Id { get; set; }

    public int Kind { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string? StudentPhotoUrl { get; set; }

    public string? StudentCode { get; set; }

    public string? Subject { get; set; }

    public string? GroupName { get; set; }

    public DateOnly? SessionDate { get; set; }

    public TimeOnly? StartTime { get; set; }

    public string? Topic { get; set; }

    public int? LessonId { get; set; }

    public int? LessonGroupId { get; set; }

    public int? SessionId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}

public sealed class TeacherReviewInboxDto
{
    public required int Total { get; init; }

    public required IReadOnlyList<TeacherReviewInboxItemDto> Items { get; init; }
}
