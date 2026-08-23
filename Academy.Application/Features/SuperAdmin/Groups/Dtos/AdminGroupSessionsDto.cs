namespace Academy.Application.Features.SuperAdmin.Groups.Dtos;

public sealed class AdminGroupSessionsDto
{
    public required int GroupId { get; init; }

    public required string GroupName { get; init; }

    public required int LessonId { get; init; }

    public required string LessonSubject { get; init; }

    public required string TeacherName { get; init; }

    public required string BillingType { get; init; }

    public decimal? SessionPrice { get; init; }

    public decimal? MonthlyPrice { get; init; }

    public required IReadOnlyList<AdminGroupSessionDto> Sessions { get; init; }
}
