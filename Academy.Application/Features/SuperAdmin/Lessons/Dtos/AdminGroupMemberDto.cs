namespace Academy.Application.Features.SuperAdmin.Lessons.Dtos;

public sealed class AdminGroupMemberDto
{
    public required int Id { get; init; }

    public required int StudentId { get; init; }

    public required string StudentName { get; init; }

    public string? StudentCode { get; init; }

    public required DateTime AddedAtUtc { get; init; }
}
