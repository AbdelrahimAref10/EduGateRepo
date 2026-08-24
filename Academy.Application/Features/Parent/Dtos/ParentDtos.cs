namespace Academy.Application.Features.Parent.Dtos;

public sealed class ParentChildDto
{
    public required int ChildStudentId { get; init; }

    public required string FullName { get; init; }

    public string? StudentCode { get; init; }

    public string? PhotoUrl { get; init; }

    public DateTime LinkedAtUtc { get; init; }
}

public sealed class ParentDashboardDto
{
    public required IReadOnlyList<ParentChildDto> Children { get; init; }

    public required IReadOnlyList<ParentUpcomingSessionDto> UpcomingSessions { get; init; }

    public required IReadOnlyList<ParentUnpaidChargeDto> UnpaidCharges { get; init; }
}

public sealed class ParentUpcomingSessionDto
{
    public required int SessionId { get; init; }

    public required int ChildStudentId { get; init; }

    public required string ChildName { get; init; }

    public required string Subject { get; init; }

    public required string GroupName { get; init; }

    public required string TeacherName { get; init; }

    public required DateOnly SessionDate { get; init; }

    public required TimeOnly StartTime { get; init; }

    public string? Topic { get; init; }

    public bool HasStarted { get; init; }
}

public sealed class ParentUnpaidChargeDto
{
    public required int ChargeId { get; init; }

    public required int ChildStudentId { get; init; }

    public required string ChildName { get; init; }

    public required string Subject { get; init; }

    public required string Type { get; init; }

    public required decimal Amount { get; init; }

    public required decimal Remaining { get; init; }

    public required string Status { get; init; }

    public DateTime CreatedAtUtc { get; init; }
}

public sealed class ParentExamListItemDto
{
    public required int ExamId { get; init; }

    public required int SessionId { get; init; }

    public required int ChildStudentId { get; init; }

    public required string ChildName { get; init; }

    public required string Title { get; init; }

    public required string Subject { get; init; }

    public required string GroupName { get; init; }

    public required string TeacherName { get; init; }

    public required DateOnly SessionDate { get; init; }

    public required TimeOnly StartTime { get; init; }

    public required int QuestionCount { get; init; }

    public required bool HasSubmitted { get; init; }

    public decimal? Score { get; init; }

    public decimal? MaxScore { get; init; }

    public decimal? Percentage { get; init; }
}

public sealed class ParentAttendanceItemDto
{
    public required int SessionId { get; init; }

    public required int ChildStudentId { get; init; }

    public required string ChildName { get; init; }

    public required string Subject { get; init; }

    public required string GroupName { get; init; }

    public required DateOnly SessionDate { get; init; }

    public required TimeOnly StartTime { get; init; }

    public required bool IsPresent { get; init; }

    public string? TeacherNotes { get; init; }
}

public sealed class ParentPaymentItemDto
{
    public required int PaymentId { get; init; }

    public required int ChildStudentId { get; init; }

    public required string ChildName { get; init; }

    public required string Subject { get; init; }

    public required decimal Amount { get; init; }

    public required string Method { get; init; }

    public required int ReceiptNumber { get; init; }

    public required DateTime PaidAtUtc { get; init; }

    public string? Note { get; init; }
}

public sealed class LinkChildRequest
{
    public required string StudentCode { get; init; }
}
