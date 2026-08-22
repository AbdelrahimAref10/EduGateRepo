namespace Academy.Application.Features.Reviews.Dtos;

public sealed class TargetReviewDto
{
    public required int Id { get; init; }

    public required int TargetId { get; init; }

    public required int StudentId { get; init; }

    public required int Rating { get; init; }

    public string? Comment { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }
}

public sealed class MyTargetReviewDto
{
    public required bool CanReview { get; init; }

    public TargetReviewDto? Review { get; init; }
}
