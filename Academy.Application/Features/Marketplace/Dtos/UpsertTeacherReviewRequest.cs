namespace Academy.Application.Features.Marketplace.Dtos;

public sealed class UpsertTeacherReviewRequest
{
    public int Rating { get; init; }

    public string? Comment { get; init; }
}
