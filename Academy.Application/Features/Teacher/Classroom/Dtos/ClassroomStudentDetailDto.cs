namespace Academy.Application.Features.Teacher.Classroom.Dtos;

public sealed class ClassroomStudentDetailDto
{
    public required int Id { get; init; }

    public required int StudentId { get; init; }

    public int? UserId { get; init; }

    public required string StudentName { get; init; }

    public string? PhotoUrl { get; init; }

    public string? StudentCode { get; init; }

    public required bool IsPresent { get; init; }

    /// <summary>Open remaining balance relevant to this classroom context.</summary>
    public required decimal OutstandingAmount { get; init; }

    /// <summary>None | Open | Partial | Paid</summary>
    public required string BillingStatus { get; init; }

    public string? TeacherNotes { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }
}
