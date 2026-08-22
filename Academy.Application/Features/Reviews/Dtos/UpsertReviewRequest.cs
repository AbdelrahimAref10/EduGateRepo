namespace Academy.Application.Features.Reviews.Dtos;

public sealed class UpsertReviewRequest
{
    public int Rating { get; init; }

    public string? Comment { get; init; }
}
