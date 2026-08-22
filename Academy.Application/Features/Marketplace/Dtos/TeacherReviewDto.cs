namespace Academy.Application.Features.Marketplace.Dtos;

public sealed class TeacherReviewDto
{
    public required int Id { get; init; }

    public required int TeacherId { get; init; }

    public required int StudentId { get; init; }

    public required int Rating { get; init; }

    public string? Comment { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }
}
